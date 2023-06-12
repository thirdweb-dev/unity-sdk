using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.AccountSigning;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;

namespace Thirdweb.AccountAbstraction
{
    public class SmartWalletAccount : IAccount
    {
        private readonly SmartWallet _wallet;
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

        public SmartWalletAccount(SmartWallet wallet, IClient client)
        {
            _wallet = wallet;
            _client = client;
            TransactionManager = new SmartWalletTransactionManager(this);
            NonceService = new InMemoryNonceService(Address, client);
            AccountSigningService = new AccountSigningService(client);
        }
    }
}
