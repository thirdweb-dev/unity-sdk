using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with a Wallet.
    /// </summary>
    public class Wallet
    {
        public Task<string> Connect() 
        {
            return Bridge.Connect();
        }

        public async Task<LoginPayload> Authenticate(string domain) 
        {
            return await Bridge.InvokeRoute<LoginPayload>("sdk#auth.login", Utils.ToJsonStringArray(domain));
        }

        public async Task<CurrencyValue> GetBalance(string currencyAddress = Utils.NativeTokenAddress)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), Utils.ToJsonStringArray(currencyAddress));
        }

        public async Task<string> GetAddress()
        {
            return await Bridge.InvokeRoute<string>(getRoute("getAddress"), new string[] { });
        }

        public async Task<bool> IsConnected()
        {
            return await Bridge.InvokeRoute<bool>(getRoute("isConnected"), new string[] { });
        }

        public async Task<int> GetChainId()
        {
            return await Bridge.InvokeRoute<int>(getRoute("getChainId"), new string[] { });
        }

        public async Task<bool> IsOnCorrectChain()
        {
            return await Bridge.InvokeRoute<bool>(getRoute("isOnCorrectChain"), new string[] { });
        }

        public void SwitchNetwork(int chainId)
        {
            Bridge.SwitchNetwork(chainId);
        }

        public async Task<TransactionResult> Transfer(string to, string amount, string currencyAddress = Utils.NativeTokenAddress)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, amount, currencyAddress));
        }

        public async Task<string> Sign(string message)
        {
            return await Bridge.InvokeRoute<string>(getRoute("sign"), Utils.ToJsonStringArray(message));
        }

        public async Task<string> RecoverAddress(string message, string signature)
        {
            return await Bridge.InvokeRoute<string>(getRoute("recoverAddress"), Utils.ToJsonStringArray(message, signature));
        }

        public async Task<TransactionResult> SendRawTransaction(TransactionRequest transactionRequest)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("sendRawTransaction"), Utils.ToJsonStringArray(transactionRequest));
        }

        /// PRIVATE

        private string getRoute(string functionPath) {
            return "sdk#wallet." + functionPath;
        }
    }
}