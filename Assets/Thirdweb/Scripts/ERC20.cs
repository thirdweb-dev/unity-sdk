using System;
using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any <c>ERC20</c> compatible contract.
    /// </summary>
    public class ERC20
    {
        public string chain;
        public string address;
        public ERC20Signature signature;

        public ERC20(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
            this.signature = new ERC20Signature(chain, address);
        }

        /// READ FUNCTIONS

        public async Task<Currency> Get()
        {
            return await Bridge.InvokeRoute<Currency>(getRoute("get"), new string[] { });
        }

        public async Task<CurrencyValue> Balance()
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), new string[] { });
        }

        public async Task<CurrencyValue> BalanceOf(string address)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balanceOf"), Utils.ToJsonStringArray(address));
        }

        public async Task<string> Allowance(string spender)
        {
            return await Bridge.InvokeRoute<string>(getRoute("allowance"), Utils.ToJsonStringArray(spender));
        }

        public async Task<string> AllowanceOf(string owner, string spender)
        {
            return await Bridge.InvokeRoute<string>(getRoute("allowanceOf"), Utils.ToJsonStringArray(owner, spender));
        }

        public async Task<CurrencyValue> TotalSupply()
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("totalSupply"), new string[] { });
        }

        /// WRITE FUNCTIONS

        public async Task<TransactionResult> SetAllowance(string spender, bool amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("setAllowance"), Utils.ToJsonStringArray(spender, amount));
        }

        public async Task<TransactionResult> Transfer(string to, string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, amount));
        }

        public async Task<TransactionResult> Burn(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(amount));
        }

        public async Task<TransactionResult[]> Claim(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), Utils.ToJsonStringArray(amount));
        }

        public async Task<TransactionResult[]> ClaimTo(string address, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), Utils.ToJsonStringArray(address, amount));
        }

        public async Task<TransactionResult> Mint(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(amount));
        }

        public async Task<TransactionResult> MintTo(string address, string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, amount));
        }

        /// PRIVATE

        private string getRoute(string functionPath) {
            return this.address + ".erc20." + functionPath;
        }
    }

    [System.Serializable]
    #nullable enable
    public class ERC20MintPayload
    {
        public string to;
        public string price;
        public string currencyAddress;
        public string primarySaleRecipient;
        public string quantity;
        public string uid;
        // TODO implement these, needs JS bridging support
        // public long mintStartTime;
        // public long mintEndTime;

        public ERC20MintPayload(string receiverAddress, string quantity) {
            this.to = receiverAddress;
            this.quantity = quantity;
            this.price = "0";
            this.currencyAddress = Utils.AddressZero;
            this.primarySaleRecipient = Utils.AddressZero;
            this.uid = Utils.ToBytes32HexString(Guid.NewGuid().ToByteArray());
            // TODO temporary solution
            // this.mintStartTime = Utils.UnixTimeNowMs() * 1000L;
            // this.mintEndTime = this.mintStartTime + 1000L * 60L * 60L * 24L * 365L;
        }
    }

    [System.Serializable]
    public struct ERC20SignedPayloadOutput
    {
        public string to;
        public string price;
        public string currencyAddress;
        public string primarySaleRecipient;
        public string quantity;
        public string uid;
        public long mintStartTime;
        public long mintEndTime;
    }

    [System.Serializable]
    public struct ERC20SignedPayload
    {
        public string signature;
        public ERC20SignedPayloadOutput payload;
    }

    public class ERC20Signature
    {
        public string chain;
        public string address;

        public ERC20Signature(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
        }

        public async Task<ERC20SignedPayload> Generate(ERC20MintPayload payloadToSign)
        {
            return await Bridge.InvokeRoute<ERC20SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
        }

        public async Task<bool> Verify(ERC20SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
        }

        public async Task<TransactionResult> Mint(ERC20SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
        }

        private string getRoute(string functionPath) {
            return this.address + ".erc20.signature." + functionPath;
        }
    }
}