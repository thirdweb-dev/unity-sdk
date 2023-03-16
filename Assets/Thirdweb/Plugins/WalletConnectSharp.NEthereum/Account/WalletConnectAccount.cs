using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.AccountSigning;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.TransactionManagers;
using WalletConnectSharp.Core;

namespace WalletConnectSharp.NEthereum.Account
{
    public class WalletConnectAccount : IAccount
    {
        private readonly WalletConnectSession _session;

        public string Address => _session.Accounts[0];

        public ITransactionManager TransactionManager { get; protected set; }
        public INonceService NonceService { get; set; }

        public IAccountSigningService AccountSigningService { get; }

        public WalletConnectAccount(WalletConnectSession session, IClient client, bool allowEthSign = false)
        {
            _session = session;
            TransactionManager = new WalletConnectTransactionManager(client, session, this, allowEthSign);
            NonceService = new InMemoryNonceService(session.Accounts[0], client);
            // TODO: ISSUE #54: Implement IAccountSigningService
        }
    }
}
