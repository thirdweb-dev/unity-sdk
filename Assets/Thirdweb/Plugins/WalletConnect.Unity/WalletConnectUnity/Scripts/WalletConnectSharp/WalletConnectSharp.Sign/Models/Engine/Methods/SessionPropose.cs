using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionPropose. Used to propose a new session
    /// to be connected to. MUST include a <see cref="RequiredNamespaces"/> and the <see cref="Participant"/> who
    /// is proposing the session
    /// </summary>
    [RpcMethod("wc_sessionPropose")]
    [RpcRequestOptions(Clock.FIVE_MINUTES, 1100)]
    public class SessionPropose : IWcMethod
    {
        /// <summary>
        /// Protocol options that should be used during the session
        /// </summary>
        [JsonProperty("relays")]
        public ProtocolOptions[] Relays { get; set; }
        
        /// <summary>
        /// The required namespaces this session will require
        /// </summary>
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        /// <summary>
        /// The optional namespaces for this session
        /// </summary>
        [JsonProperty("optionalNamespaces")]
        public Dictionary<string, RequiredNamespace> OptionalNamespaces { get; set; }
        
        /// <summary>
        /// Custom session properties for this session
        /// </summary>
        [JsonProperty("sessionProperties")]
        public Dictionary<string, string> SessionProperties { get; set; }
        
        /// <summary>
        /// The <see cref="Participant"/> that created this session proposal
        /// </summary>
        [JsonProperty("proposer")]
        public Participant Proposer { get; set; }
    }
}
