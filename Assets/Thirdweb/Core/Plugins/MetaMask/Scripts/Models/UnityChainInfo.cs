using System;
using MetaMask.Editor.NaughtyAttributes;
using MetaMask.Models;

namespace MetaMask.Unity.Models
{
    [Serializable]
    public class UnityChainInfo : ChainInfo, IValidatable
    {
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