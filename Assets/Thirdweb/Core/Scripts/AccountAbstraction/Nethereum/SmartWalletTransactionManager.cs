namespace Thirdweb.AccountAbstraction
{
    public class SmartWalletTransactionManager : Nethereum.RPC.TransactionManagers.TransactionManager
    {
        public SmartWalletTransactionManager(SmartWalletAccount account)
            : base(account.Client)
        {
            Account = account;
        }
    }
}
