namespace Thirdweb.WalletConnect
{
    public class WalletConnectTransactionManager : Nethereum.RPC.TransactionManagers.TransactionManager
    {
        public WalletConnectTransactionManager(WalletConnectAccount account)
            : base(account.Client)
        {
            Account = account;
        }
    }
}
