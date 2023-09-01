using System.Numerics;
using MetaMask.Contracts;

namespace MetaMask.Unity.Samples
{
    public interface INFTHolder
    {
        ERC721 CurrentNFT { get; set; }
        
        BigInteger TokenId { get; set; }
    }
}