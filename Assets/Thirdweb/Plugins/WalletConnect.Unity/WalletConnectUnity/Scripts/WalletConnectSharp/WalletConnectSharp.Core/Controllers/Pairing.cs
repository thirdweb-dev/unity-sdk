using System.Security.Cryptography;
using System.Text.RegularExpressions;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Expirer;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Pairing.Methods;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// A module that handles pairing two peers and storing related data
    /// </summary>
    public class Pairing : IPairing
    {
        private const int KeyLength = 32;
        private bool _initialized;
        private HashSet<string> _registeredMethods = new HashSet<string>();

        /// <summary>
        /// The name for this module instance
        /// </summary>
        public string Name
        {
            get
            {
                return $"{Core.Context}-pairing";
            }
        }

        /// <summary>
        /// The context string for this Pairing module
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// The EventDelegator this module is using for events
        /// </summary>
        public EventDelegator Events { get; }
        
        /// <summary>
        /// Get the <see cref="IStore{TKey,TValue}"/> module that is handling the storage of
        /// <see cref="PairingStruct"/> 
        /// </summary>
        public IPairingStore Store { get; }
        
        /// <summary>
        /// Get all active and inactive pairings
        /// </summary>
        public PairingStruct[] Pairings
        {
            get
            {
                return Store.Values;
            }
        }

        /// <summary>
        /// The <see cref="ICore"/> module using this module instance
        /// </summary>
        public ICore Core { get; }

        /// <summary>
        /// Create a new instance of the Pairing module using the given <see cref="ICore"/> module
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> module that is using this new Pairing module</param>
        public Pairing(ICore core)
        {
            this.Core = core;
            this.Events = new EventDelegator(this);
            this.Store = new PairingStore(core);
        }

        /// <summary>
        /// Initialize this pairing module. This will restore all active / inactive pairings
        /// from storage
        /// </summary>
        public async Task Init()
        {
            if (!_initialized)
            {
                await this.Store.Init();
                await Cleanup();
                RegisterTypedMessages();
                RegisterExpirerEvents();
                this._initialized = true;
            }
        }

        private void RegisterExpirerEvents()
        {
            this.Core.Expirer.On<Expiration>(ExpirerEvents.Expired, ExpiredCallback);
        }

        private void RegisterTypedMessages()
        {
            Core.MessageHandler.HandleMessageType<PairingDelete, bool>(OnPairingDeleteRequest, null);
            Core.MessageHandler.HandleMessageType<PairingPing, bool>(OnPairingPingRequest, OnPairingPingResponse);
        }

        /// <summary>
        /// Pair with a peer using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown. You may (optionally) pair
        /// without activating the pairing. By default the pairing will be activated before
        /// it is returned
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The pairing data that can be used to pair with the peer</returns>
        public async Task<PairingStruct> Pair(string uri, bool activatePairing = true)
        {
            IsInitialized();
            await IsValidPair(uri);
            var uriParams = ParseUri(uri);

            var topic = uriParams.Topic;
            var symKey = uriParams.SymKey;
            var relay = uriParams.Relay;

            if (this.Store.Keys.Contains(topic))
            {
                throw new ArgumentException($"Topic {topic} already has pairing");
            }

            if (await this.Core.Crypto.HasKeys(topic))
            {
                throw new ArgumentException($"Topic {topic} already has keychain");
            }
            
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
            var pairing = new PairingStruct()
            {
                Topic = topic,
                Relay = relay,
                Expiry = expiry,
                Active = false,
            };
            
            await this.Store.Set(topic, pairing);
            await this.Core.Crypto.SetSymKey(symKey, topic);
            await this.Core.Relayer.Subscribe(topic, new SubscribeOptions()
            {
                Relay = relay
            });
            
            this.Core.Expirer.Set(topic, expiry);

            if (activatePairing)
            {
                await ActivatePairing(topic);
            }

            return pairing;
        }

        /// <summary>
        /// Parse a session proposal URI and return all information in the URI in a
        /// new <see cref="UriParameters"/> object
        /// </summary>
        /// <param name="uri">The uri to parse</param>
        /// <returns>A new <see cref="UriParameters"/> object that contains all data
        /// parsed from the given uri</returns>
        public UriParameters ParseUri(string uri)
        {
            var pathStart = uri.IndexOf(":", StringComparison.Ordinal);
            int? pathEnd = uri.IndexOf("?", StringComparison.Ordinal) != -1 ? uri.IndexOf("?", StringComparison.Ordinal) : (int?)null;
            var protocol = uri.Substring(0, pathStart);

            string path;
            if (pathEnd != null) path = uri.Substring(pathStart + 1, (int)pathEnd - (pathStart + 1));
            else path = uri.Substring(pathStart + 1);

            var requiredValues = path.Split("@");
            string queryString = pathEnd != null ? uri.Substring((int)pathEnd) : "";
            var queryParams = Regex.Matches(queryString, "([^?=&]+)(=([^&]*))?").Cast<Match>()
                .ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);

            var result = new UriParameters()
            {
                Protocol = protocol,
                Topic = requiredValues[0],
                Version = int.Parse(requiredValues[1]),
                SymKey = queryParams["symKey"],
                Relay = new ProtocolOptions()
                {
                    Protocol = queryParams["relay-protocol"],
                    Data = queryParams.ContainsKey("relay-data") ? queryParams["relay-data"] : null,
                }
            };

            return result;
        }

        /// <summary>
        /// Create a new pairing at the given pairing topic
        /// </summary>
        /// <returns>A new instance of <see cref="CreatePairingData"/> that includes the pairing topic and
        /// uri</returns>
        public async Task<CreatePairingData> Create()
        {
            byte[] symKeyRaw = new byte[KeyLength];
            RandomNumberGenerator.Fill(symKeyRaw);
            var symKey = symKeyRaw.ToHex();
            var topic = await this.Core.Crypto.SetSymKey(symKey);
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
            var relay = new ProtocolOptions()
            {
                Protocol = RelayProtocols.Default
            };
            var pairing = new PairingStruct()
            {
                Topic = topic,
                Expiry = expiry,
                Relay = relay,
                Active = false,
            };
            var uri = $"{ICore.Protocol}:{topic}@{ICore.Version}?"
                .AddQueryParam("symKey", symKey)
                .AddQueryParam("relay-protocol", relay.Protocol);

            if (!string.IsNullOrWhiteSpace(relay.Data))
                uri = uri.AddQueryParam("relay-data", relay.Data);

            await this.Store.Set(topic, pairing);
            await this.Core.Relayer.Subscribe(topic);
            this.Core.Expirer.Set(topic, expiry);

            return new CreatePairingData()
            {
                Topic = topic,
                Uri = uri
            };
        }

        /// <summary>
        /// Activate a previously created pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to activate</param>
        public Task Activate(string topic)
        {
            return ActivatePairing(topic);
        }

        /// <summary>
        /// Subscribe to method requests
        /// </summary>
        /// <param name="methods">The methods to register and subscribe</param>
        public Task Register(string[] methods)
        {
            IsInitialized();
            foreach (var method in methods)
            {
                _registeredMethods.Add(method);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Update the expiration of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="expiration">The new expiration date as a unix timestamp (seconds)</param>
        /// <returns></returns>
        public Task UpdateExpiry(string topic, long expiration)
        {
            IsInitialized();
            return this.Store.Update(topic, new PairingStruct() {Expiry = expiration});
        }

        /// <summary>
        /// Update the metadata of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="metadata">The new metadata</param>
        public Task UpdateMetadata(string topic, Metadata metadata)
        {
            IsInitialized();
            return this.Store.Update(topic, new PairingStruct() {PeerMetadata = metadata});
        }

        /// <summary>
        /// Ping an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to ping</param>
        public async Task Ping(string topic)
        {
            IsInitialized();
            await IsValidPairingTopic(topic);
            if (this.Store.Keys.Contains(topic))
            {
                var id = await Core.MessageHandler.SendRequest<PairingPing, bool>(topic, new PairingPing());
                var done = new TaskCompletionSource<bool>();
                this.Events.ListenForOnce<JsonRpcResponse<bool>>($"pairing_ping{id}", (sender, e) =>
                {
                    if (e.EventData.IsError)
                        done.SetException(e.EventData.Error.ToException());
                    else
                        done.SetResult(e.EventData.Result);
                });
                await done.Task;
            }
        }

        /// <summary>
        /// Disconnect an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to disconnect</param>
        public async Task Disconnect(string topic)
        {
            IsInitialized();
            await IsValidPairingTopic(topic);

            if (Store.Keys.Contains(topic))
            {
                var error = Error.FromErrorType(ErrorType.USER_DISCONNECTED);
                await Core.MessageHandler.SendRequest<PairingDelete, bool>(topic,
                    new PairingDelete() {Code = error.Code, Message = error.Message});
                await DeletePairing(topic);
            }
        }

        private async Task ActivatePairing(string topic)
        {
            var expiry = Clock.CalculateExpiry(Clock.THIRTY_DAYS);
            await this.Store.Update(topic, new PairingStruct()
            {
                Active = true,
                Expiry = expiry
            });

            this.Core.Expirer.Set(topic, expiry);
        }
        
        private async Task DeletePairing(string topic)
        {
            bool expirerHasDeleted = !this.Core.Expirer.Has(topic);
            bool pairingHasDeleted = !this.Store.Keys.Contains(topic);
            bool symKeyHasDeleted = !(await this.Core.Crypto.HasKeys(topic));
            
            await this.Core.Relayer.Unsubscribe(topic);
            await Task.WhenAll(
                pairingHasDeleted ? Task.CompletedTask : this.Store.Delete(topic, Error.FromErrorType(ErrorType.USER_DISCONNECTED)),
                symKeyHasDeleted ? Task.CompletedTask : this.Core.Crypto.DeleteSymKey(topic),
                expirerHasDeleted ? Task.CompletedTask : this.Core.Expirer.Delete(topic)
            );
        }

        private Task Cleanup()
        {
            List<string> pairingTopics = (from pair in this.Store.Values.Where(e => e.Expiry != null) where Clock.IsExpired(pair.Expiry.Value) select pair.Topic).ToList();
            
            return Task.WhenAll(
                pairingTopics.Select(DeletePairing)
            );
        }
        
        private async Task IsValidPairingTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID,
                    $"pairing topic should be a string {topic}");

            if (!this.Store.Keys.Contains(topic))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY,
                    $"pairing topic doesn't exist {topic}");

            var expiry = this.Store.Get(topic).Expiry;
            if (expiry != null && Clock.IsExpired(expiry.Value))
            {
                await DeletePairing(topic);
                throw WalletConnectException.FromType(ErrorType.EXPIRED, $"pairing topic: {topic}");
            }
        }
        
        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            
            try
            {
                new Uri(url);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        private Task IsValidPair(string uri)
        {
            if (!IsValidUrl(uri))
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"pair() uri: {uri}");
            return Task.CompletedTask;
        }
        
        private void IsInitialized()
        {
            if (!_initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, this.Name);
            }
        }
        
        private async Task OnPairingPingRequest(string topic, JsonRpcRequest<PairingPing> payload)
        {
            var id = payload.Id;
            try
            {
                await IsValidPairingTopic(topic);

                await Core.MessageHandler.SendResult<PairingPing, bool>(id, topic, true);
                this.Events.Trigger(PairingEvents.PairingPing, new PairingEvent()
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (WalletConnectException e)
            {
                await Core.MessageHandler.SendError<PairingPing, bool>(id, topic, Error.FromException(e));
            }
        }

        private async Task OnPairingPingResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            
            // put at the end of the stack to avoid a race condition
            // where session_ping listener is not yet initialized
            await Task.Delay(500);

            this.Events.Trigger($"pairing_ping{id}", payload);
        }
        
        private async Task OnPairingDeleteRequest(string topic, JsonRpcRequest<PairingDelete> payload)
        {
            var id = payload.Id;
            try
            {
                await IsValidDisconnect(topic, payload.Params);

                await Core.MessageHandler.SendResult<PairingDelete, bool>(id, topic, true);
                await DeletePairing(topic);
                this.Events.Trigger(PairingEvents.PairingDelete, new PairingEvent()
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (WalletConnectException e)
            {
                await Core.MessageHandler.SendError<PairingDelete, bool>(id, topic, Error.FromException(e));
            }
        }
        
        private async Task IsValidDisconnect(string topic, Error reason)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw WalletConnectException.FromType(ErrorType.MISSING_OR_INVALID, $"disconnect() params: {topic}");
            }

            await IsValidPairingTopic(topic);
        }
        
        private async void ExpiredCallback(object sender, GenericEvent<Expiration> e)
        {
            var target = new ExpirerTarget(e.EventData.Target);

            if (string.IsNullOrWhiteSpace(target.Topic)) return;

            var topic = target.Topic;
            if (this.Store.Keys.Contains(topic))
            {
                await DeletePairing(topic);
                this.Events.Trigger(PairingEvents.PairingExpire, new PairingEvent()
                {
                    Topic = topic,
                });
            }
        }
    }
}
