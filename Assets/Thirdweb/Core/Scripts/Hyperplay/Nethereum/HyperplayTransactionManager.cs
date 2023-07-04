namespace Thirdweb.Hyperplay
{
    public class HyperplayTransactionManager : Nethereum.RPC.TransactionManagers.TransactionManager
    {
        public HyperplayTransactionManager(HyperplayAccount account)
            : base(account.Client)
        {
            Account = account;
        }
    }
}
