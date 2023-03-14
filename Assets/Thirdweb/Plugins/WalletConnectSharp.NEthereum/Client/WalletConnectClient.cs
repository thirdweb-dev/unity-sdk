using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.NEthereum.Client
{
    public class WalletConnectClient : ClientBase
    {
        public WalletConnectSession Session { get; }

        public WalletConnectClient(WalletConnectSession provider)
        {
            this.Session = provider;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage message, string route = null)
        {
            var id = RpcPayloadId.Generate();
            var mapParameters = message.RawParameters as Dictionary<string, object>;
            var arrayParameters = message.RawParameters as object[];
            var rawParameters = message.RawParameters;

            var rpcRequestMessage =
                mapParameters != null
                    ? new RpcRequestMessage(id, message.Method, mapParameters)
                    : arrayParameters != null
                        ? new RpcRequestMessage(id, message.Method, arrayParameters)
                        : new RpcRequestMessage(id, message.Method, rawParameters);

            var eventCompleted = new TaskCompletionSource<RpcResponseMessage>(TaskCreationOptions.None);

            Session.Events.ListenForGenericResponse<RpcResponseMessage>(
                rpcRequestMessage.Id,
                (sender, args) =>
                {
                    eventCompleted.SetResult(args.Response);
                }
            );

            await Session.SendRequest(rpcRequestMessage);

            return await eventCompleted.Task;
        }

        protected override async Task<RpcResponseMessage[]> SendAsync(RpcRequestMessage[] requests)
        {
            var responses = new List<RpcResponseMessage>();

            foreach (var request in requests)
            {
                responses.Add(await SendAsync(request));
            }

            return responses.ToArray();
        }
    }
}
