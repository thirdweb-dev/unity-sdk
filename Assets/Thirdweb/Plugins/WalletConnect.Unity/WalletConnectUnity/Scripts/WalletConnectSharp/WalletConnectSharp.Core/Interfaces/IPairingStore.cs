using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// A <see cref="IStore{TKey,TValue}"/> interface for a module
    /// that stores <see cref="PairingStruct"/> data.
    /// </summary>
    public interface IPairingStore : IStore<string, PairingStruct> { }
}
