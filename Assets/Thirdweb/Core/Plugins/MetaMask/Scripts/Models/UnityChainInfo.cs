using System;
using MetaMask.Editor.NaughtyAttributes;
using MetaMask.Models;

namespace MetaMask.Unity.Models
{
    [Serializable]
    public class UnityChainInfo : ChainInfo, IValidatable
    {
        public UnityChainInfo() : base()
        {
        }

        public UnityChainInfo(ChainInfo other) : base(other)
        {
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ChainId) &&
                   !string.IsNullOrWhiteSpace(ChainName) &&
                   BlockExplorerUrls?.Length > 0 &&
                   NativeCurrency != null &&
                   !string.IsNullOrWhiteSpace(NativeCurrency.Name) &&
                   !string.IsNullOrWhiteSpace(NativeCurrency.Symbol) &&
                   NativeCurrency.Decimals > 0;
        }
    }
}