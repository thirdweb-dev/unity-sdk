using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Thirdweb.Wallets
{
    public interface IThirdwebWallet
    {
        Task<string> Connect(WalletConnection walletConnection, string rpc);
        Task Disconnect();
        Account GetLocalAccount();
        Task<string> GetAddress();
        Task<string> GetSignerAddress();
        WalletProvider GetProvider();
        WalletProvider GetSignerProvider();
        Task<Web3> GetWeb3();
        Task<Web3> GetSignerWeb3();
        Task<bool> IsConnected();
    }
}
