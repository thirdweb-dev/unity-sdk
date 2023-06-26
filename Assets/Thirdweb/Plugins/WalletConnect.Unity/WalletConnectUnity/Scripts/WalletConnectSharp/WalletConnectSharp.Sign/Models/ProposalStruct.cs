using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A struct that stores proposal data, including the id of the proposal, when the
    /// proposal expires and other information
    /// </summary>
    public struct ProposalStruct : IKeyHolder<long>
    {
        /// <summary>
        /// The id of this proposal
        /// </summary>
        [JsonProperty("id")]
        public long? Id { get; set; }

        /// <summary>
        /// This is the key field, mapped to the Id. Implemented for <see cref="IKeyHolder{TKey}"/>
        /// so this struct can be stored using <see cref="IStore{TKey,TValue}"/>
        /// </summary>
        [JsonIgnore]
        public long Key
        {
            get
            {
                return (long) Id;
            }
        }
        
        /// <summary>
        /// When this proposal expires
        /// </summary>
        [JsonProperty("expiry")]
        public long? Expiry { get; set; }
        
        /// <summary>
        /// Relay protocol options for this proposal
        /// </summary>
        [JsonProperty("relays")]
        public ProtocolOptions[] Relays { get; set; }
        
        /// <summary>
        /// The participant that created this proposal
        /// </summary>
        [JsonProperty("proposer")]
        public Participant Proposer { get; set; }
        
        /// <summary>
        /// The required namespaces for this proposal requests
        /// </summary>
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        /// <summary>
        /// The optional namespaces for this proposal requests
        /// </summary>
        [JsonProperty("optionalNamespaces")]
        public Dictionary<string, RequiredNamespace> OptionalNamespaces { get; set; }
        
        /// <summary>
        /// Custom session properties for this proposal request
        /// </summary>
        [JsonProperty("sessionProperties")]
        public Dictionary<string, string> SessionProperties { get; set; }

        /// <summary>
        /// The pairing topic this proposal lives in
        /// </summary>
        [JsonProperty("pairingTopic")]
        public string PairingTopic { get; set; }

        /// <summary>
        /// Approve this proposal with a single address and (optional) protocol options. The
        /// protocolOption given must exist in this proposal
        /// </summary>
        /// <param name="approvedAccount">The account address that approves this proposal</param>
        /// <param name="protocolOption">(optional) The protocol option to use. If left null, then the first protocol
        /// option in this proposal will be chosen</param>
        /// <returns>The <see cref="ApproveParams"/> that can be given to <see cref="IEngineAPI.Approve(ApproveParams)"/></returns>
        public ApproveParams ApproveProposal(string approvedAccount, ProtocolOptions protocolOption = null)
        {
            return ApproveProposal(new[] { approvedAccount }, protocolOption);
        }

        /// <summary>
        /// Approve this proposal with am array of addresses and (optional) protocol options. The
        /// protocolOption given must exist in this proposal
        /// </summary>
        /// <param name="approvedAccounts">The account addresses that are approved in this proposal</param>
        /// <param name="protocolOption">(optional) The protocol option to use. If left null, then the first protocol
        /// option in this proposal will be chosen.</param>
        /// <returns>The <see cref="ApproveParams"/> that can be given to <see cref="IEngineAPI.Approve(ApproveParams)"/></returns>
        public ApproveParams ApproveProposal(string[] approvedAccounts, ProtocolOptions protocolOption = null)
        {
            if (Id == null)
                throw new Exception("Proposal has no set Id");
            if (protocolOption == null)
                protocolOption = Relays[0];
            else if (Relays.All(r => r.Protocol != protocolOption.Protocol))
                throw new Exception("Requested protocol not in proposal");

            var relayProtocol = protocolOption.Protocol;

            var namespaces = new Namespaces();
            foreach (var key in RequiredNamespaces.Keys)
            {
                var rn = RequiredNamespaces[key];
                var allAccounts = (from chain in rn.Chains from account in approvedAccounts select $"{chain}:{account}").ToArray();
                
                namespaces.Add(key, new Namespace()
                {
                    Accounts = allAccounts,
                    Events = rn.Events,
                    Methods = rn.Methods
                });
            }
            if (OptionalNamespaces != null)
            {
                foreach (var key in OptionalNamespaces.Keys)
                {
                    var rn = OptionalNamespaces[key];
                    var allAccounts = (from chain in rn.Chains from account in approvedAccounts select $"{chain}:{account}").ToArray();
                
                    namespaces.Add(key, new Namespace()
                    {
                        Accounts = allAccounts,
                        Events = rn.Events,
                        Methods = rn.Methods
                    });
                }
                
            }

            return new ApproveParams()
            {
                Id = Id.Value,
                RelayProtocol = relayProtocol,
                Namespaces = namespaces,
                SessionProperties = SessionProperties,
            };
        }

        /// <summary>
        /// Reject this proposal with the given <see cref="Error"/>. This
        /// will return a <see cref="RejectParams"/> which must be used in <see cref="IEngineAPI.Reject(RejectParams)"/>
        /// </summary>
        /// <param name="error">The error reason this proposal was rejected</param>
        /// <returns>A new <see cref="RejectParams"/> object which must be used in <see cref="IEngineAPI.Reject(RejectParams)"/></returns>
        /// <exception cref="Exception">If this proposal has no Id</exception>
        public RejectParams RejectProposal(Error error)
        {
            if (Id == null)
                throw new Exception("Proposal has no set Id");

            return new RejectParams() {Id = Id.Value, Reason = error};
        }

        /// <summary>
        /// Reject this proposal with the given message. This
        /// will return a <see cref="RejectParams"/> which must be used in <see cref="IEngineAPI.Reject(RejectParams)"/>
        /// </summary>
        /// <param name="message">The reason message this proposal was rejected</param>
        /// <returns>A new <see cref="RejectParams"/> object which must be used in <see cref="IEngineAPI.Reject(RejectParams)"/></returns>
        /// <exception cref="Exception">If this proposal has no Id</exception>
        public RejectParams RejectProposal(string message = null)
        {
            if (Id == null)
                throw new Exception("Proposal has no set Id");
            
            if (message == null)
                message = "Proposal denied by remote host";
            
            return RejectProposal(new Error()
            {
                Message = message,
                Code = (long) ErrorType.USER_DISCONNECTED
            });
        }
    }
}
