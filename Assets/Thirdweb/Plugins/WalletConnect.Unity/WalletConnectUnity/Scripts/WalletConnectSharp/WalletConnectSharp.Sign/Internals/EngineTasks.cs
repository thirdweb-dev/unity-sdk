using System.Security.Cryptography;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign
{
    public partial class Engine
    {
        async Task IEnginePrivate.DeletePendingSessionRequest(long id, Error reason, bool expirerHasDeleted = false)
        {
            await Task.WhenAll(
                this.Client.PendingRequests.Delete(id, reason),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Core.Expirer.Delete(id)
            );
        }

        async Task IEnginePrivate.SetPendingSessionRequest(PendingRequestStruct pendingRequest)
        {
            var options = RpcRequestOptionsAttribute.GetOptionsForType<SessionRequest<object>>();
            var expiry = options.TTL;

            await this.Client.PendingRequests.Set(pendingRequest.Id, pendingRequest);

            if (expiry != 0)
            {
                this.Client.Core.Expirer.Set(pendingRequest.Id, Clock.CalculateExpiry(expiry));
            }
        } 
        
        async Task IEnginePrivate.DeleteSession(string topic)
        {
            var session = this.Client.Session.Get(topic);
            var self = session.Self;
            
            bool expirerHasDeleted = !this.Client.Core.Expirer.Has(topic);
            bool sessionDeleted = !this.Client.Session.Keys.Contains(topic);
            bool hasKeypairDeleted = !(await this.Client.Core.Crypto.HasKeys(self.PublicKey));
            bool hasSymkeyDeleted = !(await this.Client.Core.Crypto.HasKeys(topic));

            await this.Client.Core.Relayer.Unsubscribe(topic);
            await Task.WhenAll(
                sessionDeleted ? Task.CompletedTask : this.Client.Session.Delete(topic, Error.FromErrorType(ErrorType.USER_DISCONNECTED)),
                hasKeypairDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteKeyPair(self.PublicKey),
                hasSymkeyDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteSymKey(topic),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Core.Expirer.Delete(topic)
            );
        }

        Task IEnginePrivate.DeleteProposal(long id)
        {
            bool expirerHasDeleted = !this.Client.Core.Expirer.Has(id);
            bool proposalHasDeleted = !this.Client.Proposal.Keys.Contains(id);
            
            return Task.WhenAll(
                proposalHasDeleted ? Task.CompletedTask : this.Client.Proposal.Delete(id, Error.FromErrorType(ErrorType.USER_DISCONNECTED)),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Core.Expirer.Delete(id)
            );
        }

        async Task IEnginePrivate.SetExpiry(string topic, long expiry)
        {
            if (this.Client.Session.Keys.Contains(topic))
            {
                await this.Client.Session.Update(topic, new SessionStruct()
                {
                    Expiry = expiry
                });
            }
            this.Client.Core.Expirer.Set(topic, expiry);
        }

        async Task IEnginePrivate.SetProposal(long id, ProposalStruct proposal)
        {
            await this.Client.Proposal.Set(id, proposal);
            if (proposal.Expiry != null)
                this.Client.Core.Expirer.Set(id, (long)proposal.Expiry);
        }

        Task IEnginePrivate.Cleanup()
        {
            List<string> sessionTopics = (from session in this.Client.Session.Values.Where(e => e.Expiry != null) where Clock.IsExpired(session.Expiry.Value) select session.Topic).ToList();
            List<long> proposalIds = (from p in this.Client.Proposal.Values.Where(e => e.Expiry != null) where Clock.IsExpired(p.Expiry.Value) select p.Id.Value).ToList();

            return Task.WhenAll(
                sessionTopics.Select(t => PrivateThis.DeleteSession(t)).Concat(
                    proposalIds.Select(id => PrivateThis.DeleteProposal(id))
                )
            );
        }

        async Task<VerifiedContext> VerifyContext(string hash, Metadata metadata)
        {
            var context = new VerifiedContext()
            {
                VerifyUrl = metadata.VerifyUrl ?? "",
                Validation = Validation.Unknown,
                Origin = metadata.Url ?? ""
            };

            try
            {
                var origin = await this.Client.Core.Verify.Resolve(hash);
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    context.Origin = origin;
                    context.Validation = origin == metadata.Url ? Validation.Valid : Validation.Invalid;
                }
            }
            catch (Exception e)
            {
                // TODO Log to logger
            }

            return context;
        }
    }
}
