using Nethereum.RPC.TransactionManagers;

namespace MetaMask.NEthereum
{
    public class MetaMaskTransactionManager : TransactionManager
    {
        public MetaMaskTransactionManager(MetaMaskAccount account) : base(account.Client)
        {
            Account = account;
        }
    }
}