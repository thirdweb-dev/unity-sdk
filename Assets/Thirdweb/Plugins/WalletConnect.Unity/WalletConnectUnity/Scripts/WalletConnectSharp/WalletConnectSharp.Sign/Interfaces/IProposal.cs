using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// A <see cref="IStore{TKey,TValue}"/> interface for a module
    /// that stores <see cref="ProposalStruct"/> data.
    /// </summary>
    public interface IProposal : IStore<long, ProposalStruct> { }
}
