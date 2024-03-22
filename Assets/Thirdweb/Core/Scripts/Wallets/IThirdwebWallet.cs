using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Thirdweb.Wallets
{
    /// <summary>
    /// Interface for interacting with a Thirdweb wallet.
    /// </summary>
    public interface IThirdwebWallet
    {
        /// <summary>
        /// Main Connect call - should fully connect to the wallet and return the address.
        /// </summary>
        /// <param name="walletConnection">The wallet connection details.</param>
        /// <param name="rpc">The RPC endpoint.</param>
        /// <returns>The address of the connected wallet.</returns>
        Task<string> Connect(WalletConnection walletConnection, string rpc);

        /// <summary>
        /// Main Disconnect call - should fully disconnect from the wallet and reset any variables.
        /// </summary>
        Task Disconnect(bool endSession = true);

        /// <summary>
        /// Get the local account if any, return null otherwise.
        /// </summary>
        /// <returns>The local account, or null if not available.</returns>
        Account GetLocalAccount();

        /// <summary>
        /// Return the address of the main account.
        /// </summary>
        /// <returns>The address of the main account.</returns>
        Task<string> GetAddress();

        /// <summary>
        /// Return the email of the main account (if any, otherwise return an empty string).
        /// </summary>
        /// <returns>The email of the main account or an empty string.</returns>
        Task<string> GetEmail();

        /// <summary>
        /// Return the address of the signer account (if any, otherwise return GetAddress).
        /// </summary>
        /// <returns>The address of the signer account.</returns>
        Task<string> GetSignerAddress();

        /// <summary>
        /// Return the WalletProvider you added above.
        /// </summary>
        /// <returns>The WalletProvider.</returns>
        WalletProvider GetProvider();

        /// <summary>
        /// Return the WalletProvider of the signer account (if any, otherwise return GetProvider).
        /// </summary>
        /// <returns>The WalletProvider of the signer account.</returns>
        WalletProvider GetSignerProvider();

        /// <summary>
        /// Return the Web3 Nethereum provider for the main account - must override Task<RpcResponseMessage> SendAsync.
        /// </summary>
        /// <returns>The Web3 Nethereum provider for the main account.</returns>
        Task<Web3> GetWeb3();

        /// <summary>
        /// Return the Web3 Nethereum provider for the signer account (if any, otherwise return GetWeb3).
        /// </summary>
        /// <returns>The Web3 Nethereum provider for the signer account.</returns>
        Task<Web3> GetSignerWeb3();

        /// <summary>
        /// Return whether the wallet is currently connected (e.g. Web3 != null).
        /// </summary>
        /// <returns>True if the wallet is connected; otherwise, false.</returns>
        Task<bool> IsConnected();

        /// <summary>
        /// Prepares the wallet for a network switch and returns an actionable response.
        /// </summary>
        /// <param name="newChainId">The new chain ID to switch to.</param>
        /// <param name="newRpc">The new RPC endpoint to switch to.</param>
        /// <returns>A <see cref="NetworkSwitchAction"/> indicating the action to be taken.</returns>
        Task<NetworkSwitchAction> PrepareForNetworkSwitch(BigInteger newChainId, string newRpc);
    }

    public enum NetworkSwitchAction
    {
        /// <summary>
        /// Indicates that the network switch can proceed. The SDK should continue with the wallet_switchEthereumChain RPC call.
        /// </summary>
        ContinueSwitch,

        /// <summary>
        /// Indicates that the wallet has already handled the network switch internally. There's no need to make the wallet_switchEthereumChain RPC call.
        /// </summary>
        Handled,

        /// <summary>
        /// Indicates that the network switching feature is completely unsupported for the current wallet implementation.
        /// </summary>
        Unsupported
    }
}
