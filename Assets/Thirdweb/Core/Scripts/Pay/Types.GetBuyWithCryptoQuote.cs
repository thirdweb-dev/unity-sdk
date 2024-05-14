using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class BuyWithCryptoQuoteParams
    {
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        [JsonProperty("fromChainId")]
        public BigInteger? FromChainId { get; set; }

        [JsonProperty("fromTokenAddress")]
        public string FromTokenAddress { get; set; }

        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        [JsonProperty("fromAmountWei")]
        public string FromAmountWei { get; set; }

        [JsonProperty("toChainId")]
        public BigInteger? ToChainId { get; set; }

        [JsonProperty("toTokenAddress")]
        public string ToTokenAddress { get; set; }

        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        [JsonProperty("maxSlippageBPS")]
        public double? MaxSlippageBPS { get; set; }

        [JsonProperty("intentId")]
        public string IntentId { get; set; }

        public BuyWithCryptoQuoteParams(
            string fromAddress,
            BigInteger? fromChainId,
            string fromTokenAddress,
            string toTokenAddress,
            string fromAmount = null,
            string fromAmountWei = null,
            BigInteger? toChainId = null,
            string toAmount = null,
            string toAmountWei = null,
            double? maxSlippageBPS = null,
            string intentId = null
        )
        {
            FromAddress = fromAddress;
            FromChainId = fromChainId;
            FromTokenAddress = fromTokenAddress;
            FromAmount = fromAmount;
            FromAmountWei = fromAmountWei;
            ToChainId = toChainId;
            ToTokenAddress = toTokenAddress;
            ToAmount = toAmount;
            ToAmountWei = toAmountWei;
            MaxSlippageBPS = maxSlippageBPS;
            IntentId = intentId;
        }
    }

    public class TransactionRequest
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        [JsonProperty("gasPrice")]
        public string GasPrice { get; set; }

        [JsonProperty("gasLimit")]
        public string GasLimit { get; set; }
    }

    public class Approval
    {
        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        [JsonProperty("tokenAddress")]
        public string TokenAddress { get; set; }

        [JsonProperty("spenderAddress")]
        public string SpenderAddress { get; set; }

        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }
    }

    public class PaymentToken
    {
        [JsonProperty("token")]
        public Token Token { get; set; }

        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountUSDCents")]
        public double AmountUSDCents { get; set; }
    }

    public class ProcessingFee
    {
        [JsonProperty("token")]
        public Token Token { get; set; }

        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountUSDCents")]
        public double AmountUSDCents { get; set; }
    }

    public class BuyWithCryptoQuoteResult
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("transactionRequest")]
        public TransactionRequest TransactionRequest { get; set; }

        [JsonProperty("approval")]
        public Approval Approval { get; set; }

        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("fromToken")]
        public Token FromToken { get; set; }

        [JsonProperty("toToken")]
        public Token ToToken { get; set; }

        [JsonProperty("fromAmountWei")]
        public string FromAmountWei { get; set; }

        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        [JsonProperty("toAmountMinWei")]
        public string ToAmountMinWei { get; set; }

        [JsonProperty("toAmountMin")]
        public string ToAmountMin { get; set; }

        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        [JsonProperty("paymentTokens")]
        public List<PaymentToken> PaymentTokens { get; set; }

        [JsonProperty("processingFees")]
        public List<ProcessingFee> ProcessingFees { get; set; }

        [JsonProperty("estimated")]
        public Estimated Estimated { get; set; }

        [JsonProperty("maxSlippageBPS")]
        public double MaxSlippageBPS { get; set; }

        [JsonProperty("bridge")]
        public string Bridge { get; set; }
    }

    public class GetSwapQuoteResponse
    {
        [JsonProperty("result")]
        public BuyWithCryptoQuoteResult Result { get; set; }
    }
}
