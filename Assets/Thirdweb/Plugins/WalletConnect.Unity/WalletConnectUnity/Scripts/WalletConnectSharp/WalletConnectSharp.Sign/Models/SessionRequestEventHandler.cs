using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A sub-class of <see cref="TypedEventHandler{T,TR}"/> that fixes complex nesting issue with
    /// SessionRequest<T>. The purpose of this class is to un-nest the SessionRequest<T> object
    /// </summary>
    /// <typeparam name="T">The type of the session request</typeparam>
    /// <typeparam name="TR">The type of the response for the session request</typeparam>
    public class SessionRequestEventHandler<T, TR> : TypedEventHandler<T, TR>
    {
        private IEnginePrivate _enginePrivate;
        
        /// <summary>
        /// Get a singleton instance of this class for the given <see cref="IEngine"/> context. The context
        /// string of the given <see cref="IEngine"/> will be used to determine the singleton instance to
        /// return (or if a new one needs to be created). Beware that multiple <see cref="IEngine"/> instances
        /// with the same context string will share the same event handlers.
        /// </summary>
        /// <param name="engine">The engine this singleton instance is for, and where the context string will
        /// be read from</param>
        /// <returns>The singleton instance to use for request/response event handlers</returns>
        public static new TypedEventHandler<T, TR> GetInstance(ICore engine, IEnginePrivate _enginePrivate)
        {
            var context = engine.Context;
            
            if (_instances.ContainsKey(context)) return _instances[context];

            var _instance = new SessionRequestEventHandler<T, TR>(engine, _enginePrivate);
            
            _instances.Add(context, _instance);

            return _instance;
        }
        
        protected SessionRequestEventHandler(ICore engine, IEnginePrivate enginePrivate) : base(engine)
        {
            this._enginePrivate = enginePrivate;
        }

        protected override TypedEventHandler<T, TR> BuildNew(ICore _ref, Func<RequestEventArgs<T, TR>, bool> requestPredicate, Func<ResponseEventArgs<TR>, bool> responsePredicate)
        {
            return new SessionRequestEventHandler<T, TR>(_ref, _enginePrivate)
            {
                requestPredicate = requestPredicate,
                responsePredicate = responsePredicate
            };
        }

        protected override void Setup()
        {
            var wrappedRef = TypedEventHandler<SessionRequest<T>, TR>.GetInstance(_ref);
            
            wrappedRef.OnRequest += WrappedRefOnOnRequest;
            wrappedRef.OnResponse += WrappedRefOnOnResponse;
        }

        private Task WrappedRefOnOnResponse(ResponseEventArgs<TR> e)
        {
            return base.ResponseCallback(e.Topic, e.Response);
        }

        private async Task WrappedRefOnOnRequest(RequestEventArgs<SessionRequest<T>, TR> e)
        {
            //Set inner request id to match outer request id
            e.Request.Params.Request.Id = e.Request.Id;
            
            //Add to pending requests
            //We can't do a simple cast, so we need to copy all the data
            await _enginePrivate.SetPendingSessionRequest(new PendingRequestStruct()
            {
                Id = e.Request.Id, Parameters = new SessionRequest<object>()
                {
                    ChainId = e.Request.Params.ChainId,
                    Request = new JsonRpcRequest<object>()
                    {
                        Id = e.Request.Params.Request.Id,
                        Method = e.Request.Params.Request.Method,
                        Params = e.Request.Params.Request.Params
                    }
                }, Topic = e.Topic
            });
            
            await base.RequestCallback(e.Topic, e.Request.Params.Request);
        }
    }
}
