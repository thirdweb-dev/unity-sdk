using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Verify;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    public class SessionProposalEvent
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("params")]
        public ProposalStruct Proposal { get; set; }
        
        [JsonProperty("verifyContext")]
        public VerifiedContext VerifiedContext { get; set; }
    }
}
