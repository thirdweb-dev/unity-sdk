using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.AccountSigning;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;

namespace Thirdweb.WalletConnect
{
    public class WalletConnectAccount : IAccount
    {
        private readonly WalletConnect _wallet;
        private readonly IClient _client;

        public string Address
        {
            get { return _wallet.Accounts[0]; }
        }

        public ITransactionManager TransactionManager { get; }
        public INonceService NonceService { get; set; }
        public IAccountSigningService AccountSigningService { get; }

        public IClient Client
        {
            get { return _client; }
        }

        public WalletConnectAccount(WalletConnect wallet, IClient client)
        {
            _wallet = wallet;
            _client = client;
            TransactionManager = new WalletConnectTransactionManager(this);
            NonceService = new InMemoryNonceService(Address, client);
            AccountSigningService = new AccountSigningService(client);
        }
    }
}
