using Newtonsoft.Json;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
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
        private void IsInitialized()
        {
            if (!_initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }
        
        async Task IEnginePrivate.IsValidConnect(ConnectOptions options)
        {
            if (options == null)
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"Connect() params: {JsonConvert.SerializeObject(options)}");

            var pairingTopic = options.PairingTopic;
            var requiredNamespaces = options.RequiredNamespaces;
            var relays = options.Relays;

            if (pairingTopic != null)
                await IsValidPairingTopic(pairingTopic);
        }

        async Task IsValidPairingTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"pairing topic should be a string {topic}");

            if (!this.Client.Core.Pairing.Store.Keys.Contains(topic))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"pairing topic doesn't exist {topic}");

            var expiry = this.Client.Core.Pairing.Store.Get(topic).Expiry;
            if (expiry != null && Clock.IsExpired(expiry.Value))
            {
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"pairing topic: {topic}");
            }
        }

        async Task IsValidSessionTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"session topic should be a string {topic}");
            
            if (!this.Client.Session.Keys.Contains(topic))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"session topic doesn't exist {topic}");

            var expiry = this.Client.Session.Get(topic).Expiry;
            if (expiry != null && Clock.IsExpired(expiry.Value))
            {
                await PrivateThis.DeleteSession(topic);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"session topic: {topic}");
            }
        }

        async Task IsValidProposalId(long id)
        {
            if (!this.Client.Proposal.Keys.Contains(id))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"proposal id doesn't exist {id}");

            var expiry = this.Client.Proposal.Get(id).Expiry;
            if (expiry != null && Clock.IsExpired(expiry.Value))
            {
                await PrivateThis.DeleteProposal(id);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"proposal id: {id}");
            }
        }

        async Task IsValidSessionOrPairingTopic(string topic)
        {
            if (this.Client.Session.Keys.Contains(topic)) await this.IsValidSessionTopic(topic);
            else if (this.Client.Core.Pairing.Store.Keys.Contains(topic)) await this.IsValidPairingTopic(topic);
            else if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"session or pairing topic should be a string {topic}");
            else
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"session or pairing topic doesn't exist {topic}");
            }
        }

        Task IEnginePrivate.IsValidPair(string uri)
        {
            if (!Utils.IsValidUrl(uri))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"pair() uri: {uri}");
            return Task.CompletedTask;
        }

        Task IEnginePrivate.IsValidSessionSettleRequest(SessionSettle settle)
        {
            if (settle == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"onSessionSettleRequest() params: {settle}");
            }

            var relay = settle.Relay;
            var controller = settle.Controller;
            var namespaces = settle.Namespaces;
            var expiry = settle.Expiry;
            if (relay.Protocol != null && string.IsNullOrWhiteSpace(relay.Protocol))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"OnSessionSettleRequest() relay protocol should be a string");
            }

            if (string.IsNullOrWhiteSpace(controller.PublicKey))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    "OnSessionSettleRequest controller public key should be a string");
            }

            var validNamespacesError = IsValidNamespaces(namespaces, "OnSessionSettleRequest()");
            if (validNamespacesError != null)
                throw validNamespacesError.ToException();

            if (Clock.IsExpired(expiry))
                throw WalletConnectException.FromType(ErrorType.EXPIRED, "OnSessionSettleRequest()");
            return Task.CompletedTask;
        }

        async Task IEnginePrivate.IsValidApprove(ApproveParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"approve() params: {@params}");
            }

            var id = @params.Id;
            var namespaces = @params.Namespaces;
            var relayProtocol = @params.RelayProtocol;
            var properties = @params.SessionProperties;
            
            await IsValidProposalId(id);
            var proposal = this.Client.Proposal.Get(id);

            var validNamespacesError = IsValidNamespaces(namespaces, "approve()");
            if (validNamespacesError != null)
                throw validNamespacesError.ToException();

            var conformingNamespacesError = IsConformingNamespaces(proposal.RequiredNamespaces, namespaces, "update()");
            if (conformingNamespacesError != null)
                throw conformingNamespacesError.ToException();

            if (relayProtocol != null)
            {
                if (string.IsNullOrWhiteSpace(relayProtocol))
                    throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                        $"approve() relayProtocol: {relayProtocol}");
            }

            if (@params.SessionProperties != null)
            {
                if (@params.SessionProperties.Values.Any(string.IsNullOrWhiteSpace))
                    throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                        $"sessionProperties must be in Dictionary<string, string> format with no null or " +
                        $"empty/whitespace values. Received: {JsonConvert.SerializeObject(@params.SessionProperties)}");
            }
        }

        async Task IEnginePrivate.IsValidReject(RejectParams @params)
        {
            if (@params == null)
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"reject() params: {@params}");
            }

            var id = @params.Id;
            var reason = @params.Reason;

            await IsValidProposalId(id);

            if (reason == null || string.IsNullOrWhiteSpace(reason.Message))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"reject() reason: ${JsonConvert.SerializeObject(reason)}");
            }
        }

        async Task IEnginePrivate.IsValidUpdate(string topic, Namespaces namespaces)
        {
            await IsValidSessionTopic(topic);

            var session = this.Client.Session.Get(topic);

            var validNamespaceError = IsValidNamespaces(namespaces, "update()");
            if (validNamespaceError != null)
                throw validNamespaceError.ToException();

            var conformingNamespacesError = IsConformingNamespaces(session.RequiredNamespaces, namespaces, "update()");

            if (conformingNamespacesError != null)
                throw conformingNamespacesError.ToException();
        }

        async Task IEnginePrivate.IsValidExtend(string topic)
        {
            await IsValidSessionTopic(topic);
        }

        async Task IEnginePrivate.IsValidRequest<T>(string topic, JsonRpcRequest<T> request, string chainId)
        {
            await IsValidSessionTopic(topic);

            var session = this.Client.Session.Get(topic);
            var namespaces = session.Namespaces;
            if (!IsValidNamespacesChainId(namespaces, chainId))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"request() chainId: {chainId}");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Method))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"request() ${JsonConvert.SerializeObject(request)}");
            }

            var validMethods = GetNamespacesMethodsForChainId(namespaces, chainId);
            if (!validMethods.Contains(request.Method))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"request() method: {request.Method}");
        }

        async Task IEnginePrivate.IsValidRespond<T>(string topic, JsonRpcResponse<T> response)
        {
            await IsValidSessionTopic(topic);

            if (response == null || (response.Result == null && response.Error == null))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"respond() response: ${JsonConvert.SerializeObject(response)}");
            }
        }

        async Task IEnginePrivate.IsValidPing(string topic)
        {
            await IsValidSessionOrPairingTopic(topic);
        }

        private List<string> GetNamespacesEventsForChainId(Namespaces namespaces, string chainId)
        {
            var events = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                var chains = GetAccountsChains(ns.Accounts);
                if (chains.Contains(chainId)) events.AddRange(ns.Events);
            }

            return events;
        }

        async Task IEnginePrivate.IsValidEmit<T>(string topic, EventData<T> @event, string chainId)
        {
            await IsValidSessionTopic(topic);
            var session = this.Client.Session.Get(topic);
            var namespaces = session.Namespaces;

            if (!IsValidNamespacesChainId(namespaces, chainId))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"emit() chainId: {chainId}");
            }

            if (@event == null || string.IsNullOrWhiteSpace(@event.Name))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"emit() event: {JsonConvert.SerializeObject(@event)}");

            if (!GetNamespacesEventsForChainId(namespaces, chainId).Contains(@event.Name))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"emit() event: {JsonConvert.SerializeObject(@event)}");
            }
        }

        async Task IEnginePrivate.IsValidDisconnect(string topic, Error reason)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"disconnect() params: {topic}");
            }

            await IsValidSessionOrPairingTopic(topic);
        }

        private bool IsValidAccountId(string account)
        {
            if (!string.IsNullOrWhiteSpace(account) && account.Contains(":"))
            {
                var split = account.Split(":");
                if (split.Length == 3)
                {
                    var chainId = split[0] + ":" + split[1];
                    return !string.IsNullOrWhiteSpace(split[2]) && IsValidChainId(chainId);
                }
            }
            return false;
        }

        private Error IsValidAccounts(string[] accounts, string context)
        {
            Error error = null;
            foreach (var account in accounts)
            {
                if (error != null)
                    break;

                if (!IsValidAccountId(account))
                {
                    error = Error.FromErrorType(ErrorType.UNSUPPORTED_ACCOUNTS, $"{context}, account {account} should be a string and conform to 'namespace:chainId:address' format");
                }
            }

            return error;
        }

        private Error IsValidNamespaceAccounts(Namespaces namespaces, string method)
        {
            Error error = null;
            foreach (var ns in namespaces.Values)
            {
                if (error != null) break;

                var validAccountsError = IsValidAccounts(ns.Accounts, $"{method} namespace");
                if (validAccountsError != null)
                {
                    error = validAccountsError;
                }
            }

            return error;
        }

        private Error IsValidNamespaces(Namespaces namespaces, string method)
        {
            Error error = null;
            if (namespaces != null)
            {
                var validAccountsError = IsValidNamespaceAccounts(namespaces, method);
                if (validAccountsError != null)
                {
                    error = validAccountsError;
                }
            }
            else
            {
                error = Error.FromErrorType(ErrorType.MISSING_OR_INVALID, $"{method}, namespaces should be an object with data");
            }

            return error;
        }

        private List<string> GetNamespacesMethodsForChainId(Namespaces namespaces, string chainId)
        {
            var methods = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                var chains = GetAccountsChains(ns.Accounts);
                if (chains.Contains(chainId)) methods.AddRange(ns.Methods);
            }

            return methods;
        }

        private bool IsValidChainId(string chainId)
        {
            if (chainId.Contains(":"))
            {
                var split = chainId.Split(":");
                return split.Length == 2;
            }

            return false;
        }

        private List<string> GetNamespacesChains(Namespaces namespaces)
        {
            List<string> chains = new List<string>();
            foreach (var ns in namespaces.Values)
            {
                chains.AddRange(GetAccountsChains(ns.Accounts));
            }

            return chains;
        }

        private bool IsValidNamespacesChainId(Namespaces namespaces, string chainId)
        {
            if (!IsValidChainId(chainId)) return false;
            var chains = GetNamespacesChains(namespaces);
            if (!chains.Contains(chainId)) return false;

            return true;
        }

        private Error IsConformingNamespaces(RequiredNamespaces requiredNamespaces, Namespaces namespaces,
            string context)
        {
            Error error = null;
            var requiredNamespaceKeys = requiredNamespaces.Keys.ToArray();
            var namespaceKeys = namespaces.Keys.ToArray();
            
            if (!HasOverlap(requiredNamespaceKeys, namespaceKeys))
                error = Error.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces keys don't satisfy requiredNamespaces");
            else
            {
                foreach (var key in requiredNamespaceKeys)
                {
                    if (error != null)
                        break;

                    var requiredNamespaceChains = requiredNamespaces[key].Chains;
                    var namespaceChains = GetAccountsChains(namespaces[key].Accounts);

                    if (!HasOverlap(requiredNamespaceChains, namespaceChains))
                    {
                        error = Error.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces accounts don't satisfy requiredNamespaces chains for {key}");
                    } 
                    else if (!HasOverlap(requiredNamespaces[key].Methods, namespaces[key].Methods))
                    {
                        error = Error.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces methods don't satisfy requiredNamespaces methods for {key}");
                    }
                    else if (!HasOverlap(requiredNamespaces[key].Events, namespaces[key].Events))
                    {
                        error = Error.FromErrorType(ErrorType.NON_CONFORMING_NAMESPACES, $"{context} namespaces events don't satisfy requiredNamespaces events for {key}");
                    }
                }
            }

            return error;
        }
        
        private bool HasOverlap(string[] a, string[] b)
        {
            var matches = a.Where(x => b.Contains(x));
            return matches.Count() == a.Length;
        }

        private string[] GetAccountsChains(string[] accounts)
        {
            List<string> chains = new List<string>();
            foreach (var account in accounts)
            {
                var values = account.Split(":");
                var chain = values[0];
                var chainId = values[1];
                
                chains.Add($"{chain}:{chainId}");
            }

            return chains.ToArray();
        }

        private bool IsSessionCompatible(SessionStruct session, RequiredNamespaces requiredNamespaces)
        {
            var compatible = true;

            var sessionKeys = session.Namespaces.Keys.ToArray();
            var paramsKeys = requiredNamespaces.Keys.ToArray();

            if (!HasOverlap(paramsKeys, sessionKeys)) return false;

            foreach (var key in sessionKeys)
            {
                var value = session.Namespaces[key];
                var accounts = value.Accounts;
                var methods = value.Methods;
                var events = value.Events;
                var chains = GetAccountsChains(accounts);
                var requiredNamespace = requiredNamespaces[key];

                if (!HasOverlap(requiredNamespace.Chains, chains) ||
                    !HasOverlap(requiredNamespace.Methods, methods) ||
                    !HasOverlap(requiredNamespace.Events, events))
                    compatible = false;
            }

            return compatible;
        }
    }
}
