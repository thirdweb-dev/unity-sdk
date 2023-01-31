using System;
using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any ERC20 compatible contract.
    /// </summary>
    public class ERC20 : Routable
    {
        /// <summary>
        /// Handle signature minting functionality
        /// </summary>
        public ERC20Signature signature;
        /// <summary>
        /// Query claim conditions
        /// </summary>
        public ERC20ClaimConditions claimConditions;

        /// <summary>
        /// Interact with any ERC20 compatible contract.
        /// </summary>
        public ERC20(string parentRoute) : base(Routable.append(parentRoute, "erc20"))
        {
            this.signature = new ERC20Signature(baseRoute);
            this.claimConditions = new ERC20ClaimConditions(baseRoute);
        }

        // READ FUNCTIONS

        /// <summary>
        /// Get the currency information
        /// </summary>
        public async Task<Currency> Get()
        {
            return await Bridge.InvokeRoute<Currency>(getRoute("get"), new string[] { });
        }

        /// <summary>
        /// Get the balance of the connected wallet
        /// </summary>
        public async Task<CurrencyValue> Balance()
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), new string[] { });
        }

        /// <summary>
        /// Get the balance of the specified wallet
        /// </summary>
        public async Task<CurrencyValue> BalanceOf(string address)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balanceOf"), Utils.ToJsonStringArray(address));
        }

        /// <summary>
        /// Get how much allowance the given address is allowed to spend on behalf of the connected wallet
        /// </summary>
        public async Task<CurrencyValue> Allowance(string spender)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("allowance"), Utils.ToJsonStringArray(spender));
        }

        /// <summary>
        /// Get how much allowance the given address is allowed to spend on behalf of the specified wallet
        /// </summary>
        public async Task<CurrencyValue> AllowanceOf(string owner, string spender)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("allowanceOf"), Utils.ToJsonStringArray(owner, spender));
        }

        /// <summary>
        /// Get the total supply in circulation
        /// </summary>
        public async Task<CurrencyValue> TotalSupply()
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("totalSupply"), new string[] { });
        }

        // WRITE FUNCTIONS

        /// <summary>
        /// Set how much allowance the given address is allowed to spend on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetAllowance(string spender, string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("setAllowance"), Utils.ToJsonStringArray(spender, amount));
        }

        /// <summary>
        /// Transfer a given amount of currency to another wallet
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, amount));
        }

        /// <summary>
        /// Burn a given amount of currency
        /// </summary>
        public async Task<TransactionResult> Burn(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(amount));
        }

        /// <summary>
        /// Claim a given amount of currency for compatible drop contracts
        /// </summary>
        public async Task<TransactionResult> Claim(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("claim"), Utils.ToJsonStringArray(amount));
        }

        /// <summary>
        /// Claim a given amount of currency to a given destination wallet for compatible drop contracts
        /// </summary>
        public async Task<TransactionResult> ClaimTo(string address, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("claimTo"), Utils.ToJsonStringArray(address, amount));
        }

        /// <summary>
        /// Mint a given amount of currency
        /// </summary>
        public async Task<TransactionResult> Mint(string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(amount));
        }

        /// <summary>
        /// Mint a given amount of currency to a given destination wallet
        /// </summary>
        public async Task<TransactionResult> MintTo(string address, string amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, amount));
        }
    }

    [System.Serializable]
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

        public ERC20MintPayload(string receiverAddress, string quantity)
        {
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

    /// <summary>
    /// Fetch claim conditions for a given ERC20 drop contract
    /// </summary>
#nullable enable
    public class ERC20ClaimConditions : Routable
    {
        public ERC20ClaimConditions(string parentRoute) : base(Routable.append(parentRoute, "claimConditions"))
        {
        }

        /// <summary>
        /// Get the active claim condition
        /// </summary>
        public async Task<ClaimConditions> GetActive()
        {
            return await Bridge.InvokeRoute<ClaimConditions>(getRoute("getActive"), new string[] { });
        }

        /// <summary>
        /// Check whether the connected wallet is eligible to claim
        /// </summary>
        public async Task<bool> CanClaim(string quantity, string? addressToCheck = null)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("canClaim"), Utils.ToJsonStringArray(quantity, addressToCheck));
        }

        /// <summary>
        /// Get the reasons why the connected wallet is not eligible to claim
        /// </summary>
        public async Task<string[]> GetIneligibilityReasons(string quantity, string? addressToCheck = null)
        {
            return await Bridge.InvokeRoute<string[]>(getRoute("getClaimIneligibilityReasons"), Utils.ToJsonStringArray(quantity, addressToCheck));
        }

        /// <summary>
        /// Get the special values set in the allowlist for the given wallet
        /// </summary>
        public async Task<bool> GetClaimerProofs(string claimerAddress)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("getClaimerProofs"), Utils.ToJsonStringArray(claimerAddress));
        }
    }


    /// <summary>
    /// Generate, verify and mint signed mintable payloads
    /// </summary>
    public class ERC20Signature : Routable
    {
        /// <summary>
        /// Generate, verify and mint signed mintable payloads
        /// </summary>
        public ERC20Signature(string parentRoute) : base(Routable.append(parentRoute, "signature"))
        {
        }

        /// <summary>
        /// Generate a signed mintable payload. Requires minting permission.
        /// </summary>
        public async Task<ERC20SignedPayload> Generate(ERC20MintPayload payloadToSign)
        {
            return await Bridge.InvokeRoute<ERC20SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
        }

        /// <summary>
        /// Verify that a signed mintable payload is valid
        /// </summary>
        public async Task<bool> Verify(ERC20SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
        }

        /// <summary>
        /// Mint a signed mintable payload
        /// </summary>
        public async Task<TransactionResult> Mint(ERC20SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
        }
    }
}