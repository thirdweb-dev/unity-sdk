// using System;
// using System.Numerics;
// using Nethereum.RPC.Eth.DTOs;
// using Newtonsoft.Json;

// // all types using JsonProperty
// namespace Thirdweb.Pay
// {
//     [Serializable]
//     internal class PayResult<T>
//     {
//         [JsonProperty("result")]
//         internal T Result { get; set; }
//     }

//     // export type SwapRouteParams = {
//     //   client: ThirdwebClient;
//     //   fromAddress: string;
//     //   fromChainId: number;
//     //   fromTokenAddress: string;
//     //   fromAmountWei?: string;
//     //   toAddress?: string;
//     //   toChainId: number;
//     //   toTokenAddress: string;
//     //   toAmountWei?: string;
//     //   maxSlippageBPS?: number;
//     // };

//     [Serializable]
//     public class SwapRouteParams
//     {
//         [JsonProperty("fromAddress")]
//         public string FromAddress { get; set; }

//         [JsonProperty("fromChainId")]
//         public BigInteger FromChainId { get; set; }

//         [JsonProperty("fromTokenAddress")]
//         public string FromTokenAddress { get; set; }

//         [JsonProperty("fromAmountWei", NullValueHandling = NullValueHandling.Ignore)]
//         public string FromAmountWei { get; set; }

//         [JsonProperty("toAddress", NullValueHandling = NullValueHandling.Ignore)]
//         public string ToAddress { get; set; }

//         [JsonProperty("toChainId")]
//         public BigInteger ToChainId { get; set; }

//         [JsonProperty("toTokenAddress")]
//         public string ToTokenAddress { get; set; }

//         [JsonProperty("toAmountWei", NullValueHandling = NullValueHandling.Ignore)]
//         public string ToAmountWei { get; set; }

//         [JsonProperty("maxSlippageBPS", NullValueHandling = NullValueHandling.Ignore)]
//         public BigInteger MaxSlippageBPS { get; set; }
//     }

//     // type Approval = {
//     //   chainId: number;
//     //   tokenAddress: string;
//     //   spenderAddress: string;
//     //   amountWei: string;
//     // };

//     [Serializable]
//     public class Approval
//     {
//         [JsonProperty("chainId")]
//         public BigInteger ChainId { get; set; }

//         [JsonProperty("tokenAddress")]
//         public string TokenAddress { get; set; }

//         [JsonProperty("spenderAddress")]
//         public string SpenderAddress { get; set; }

//         [JsonProperty("amountWei")]
//         public string AmountWei { get; set; }
//     }

//     // export type SwapToken = {
//     //   chainId: number;
//     //   tokenAddress: string;
//     //   decimals: number;
//     //   priceUSDCents: number;
//     //   name?: string;
//     //   symbol?: string;
//     // };

//     [Serializable]
//     public class SwapToken
//     {
//         [JsonProperty("chainId")]
//         public BigInteger ChainId { get; set; }

//         [JsonProperty("tokenAddress")]
//         public string TokenAddress { get; set; }

//         [JsonProperty("decimals")]
//         public BigInteger Decimals { get; set; }

//         [JsonProperty("priceUSDCents")]
//         public BigInteger PriceUSDCents { get; set; }

//         [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
//         public string Name { get; set; }

//         [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
//         public string Symbol { get; set; }
//     }

//     // type PaymentToken = {
//     //   token: SwapToken;
//     //   amountWei: string;
//     // };

//     [Serializable]
//     public class PaymentToken
//     {
//         [JsonProperty("token")]
//         public SwapToken Token { get; set; }

//         [JsonProperty("amountWei")]
//         public string AmountWei { get; set; }
//     }

//     // type TransactionRequest = {
//     //   data: string;
//     //   to: string;
//     //   value: string;
//     //   from: string;
//     //   chainId: number;
//     //   gasPrice: string;
//     //   gasLimit: string;
//     // };

//     [Serializable]
//     public class TransactionRequest
//     {
//         [JsonProperty("data")]
//         public string Data { get; set; }

//         [JsonProperty("to")]
//         public string To { get; set; }

//         [JsonProperty("value")]
//         public string Value { get; set; }

//         [JsonProperty("from")]
//         public string From { get; set; }

//         [JsonProperty("chainId")]
//         public BigInteger ChainId { get; set; }

//         [JsonProperty("gasPrice")]
//         public string GasPrice { get; set; }

//         [JsonProperty("gasLimit")]
//         public string GasLimit { get; set; }
//     }

//     // type SwapRouteResponse = {
//     //   transactionId: string;
//     //   transactionRequest: TransactionRequest;
//     //   approval?: Approval;

//     //   fromAddress: string;
//     //   toAddress: string;
//     //   fromToken: SwapToken;
//     //   toToken: SwapToken;
//     //   fromAmountWei: string;
//     //   toAmountMinWei: string;
//     //   toAmountWei: string;
//     //   requiredTokens: PaymentToken[];

//     //   estimated: {
//     //     fromAmountUSDCents: number;
//     //     toAmountMinUSDCents: number;
//     //     toAmountUSDCents: number;
//     //     feesUSDCents: number;
//     //     gasCostUSDCents: number;
//     //     durationSeconds?: number;
//     //   };
//     // };

//     [Serializable]
//     internal class SwapRouteResponse
//     {
//         [JsonProperty("transactionId")]
//         internal string TransactionId { get; set; }

//         [JsonProperty("transactionRequest")]
//         internal TransactionRequest TransactionRequest { get; set; }

//         [JsonProperty("approval", NullValueHandling = NullValueHandling.Ignore)]
//         internal Approval Approval { get; set; }

//         [JsonProperty("fromAddress")]
//         internal string FromAddress { get; set; }

//         [JsonProperty("toAddress")]
//         internal string ToAddress { get; set; }

//         [JsonProperty("fromToken")]
//         internal SwapToken FromToken { get; set; }

//         [JsonProperty("toToken")]
//         internal SwapToken ToToken { get; set; }

//         [JsonProperty("fromAmountWei")]
//         internal string FromAmountWei { get; set; }

//         [JsonProperty("toAmountMinWei")]
//         internal string ToAmountMinWei { get; set; }

//         [JsonProperty("toAmountWei")]
//         internal string ToAmountWei { get; set; }

//         [JsonProperty("requiredTokens")]
//         internal PaymentToken[] RequiredTokens { get; set; }

//         [JsonProperty("estimated")]
//         internal Estimated Estimated { get; set; }
//     }

//     [Serializable]
//     public class Estimated
//     {
//         [JsonProperty("fromAmountUSDCents")]
//         public BigInteger FromAmountUSDCents { get; set; }

//         [JsonProperty("toAmountMinUSDCents")]
//         public BigInteger ToAmountMinUSDCents { get; set; }

//         [JsonProperty("toAmountUSDCents")]
//         public BigInteger ToAmountUSDCents { get; set; }

//         [JsonProperty("feesUSDCents")]
//         public BigInteger FeesUSDCents { get; set; }

//         [JsonProperty("gasCostUSDCents")]
//         public BigInteger GasCostUSDCents { get; set; }

//         [JsonProperty("durationSeconds", NullValueHandling = NullValueHandling.Ignore)]
//         public BigInteger DurationSeconds { get; set; }
//     }

//     // export type SwapRoute = {
//     //   transactionId: string;
//     //   transactionRequest: TransactionRequest;
//     //   approval?: BaseTransactionOptions<ApproveParams>;

//     //   swapDetails: {
//     //     fromAddress: string;
//     //     toAddress: string;
//     //     fromToken: SwapToken;
//     //     toToken: SwapToken;
//     //     fromAmountWei: string;
//     //     toAmountMinWei: string;
//     //     toAmountWei: string;

//     //     estimated: {
//     //       fromAmountUSDCents: number;
//     //       toAmountMinUSDCents: number;
//     //       toAmountUSDCents: number;
//     //       feesUSDCents: number;
//     //       gasCostUSDCents: number;
//     //       durationSeconds?: number;
//     //     };
//     //   };

//     //   paymentTokens: PaymentToken[];
//     //   client: ThirdwebClient;
//     // };

//     [Serializable]
//     public class SwapRoute
//     {
//         [JsonProperty("transactionId")]
//         public string TransactionId { get; set; }

//         [JsonProperty("transactionRequest")]
//         public TransactionRequest TransactionRequest { get; set; }

//         [JsonProperty("approval", NullValueHandling = NullValueHandling.Ignore)]
//         public Approval Approval { get; set; }

//         [JsonProperty("swapDetails")]
//         public SwapDetails SwapDetails { get; set; }

//         [JsonProperty("paymentTokens")]
//         public PaymentToken[] PaymentTokens { get; set; }
//     }

//     [Serializable]
//     public class SwapDetails
//     {
//         [JsonProperty("fromAddress")]
//         public string FromAddress { get; set; }

//         [JsonProperty("toAddress")]
//         public string ToAddress { get; set; }

//         [JsonProperty("fromToken")]
//         public SwapToken FromToken { get; set; }

//         [JsonProperty("toToken")]
//         public SwapToken ToToken { get; set; }

//         [JsonProperty("fromAmountWei")]
//         public string FromAmountWei { get; set; }

//         [JsonProperty("toAmountMinWei")]
//         public string ToAmountMinWei { get; set; }

//         [JsonProperty("toAmountWei")]
//         public string ToAmountWei { get; set; }

//         [JsonProperty("estimated")]
//         public Estimated Estimated { get; set; }
//     }

//     // type TransactionDetails = {
//     //   transactionHash: string;
//     //   token: SwapToken;
//     //   amountWei: string;
//     //   amountUSDCents: number;
//     //   completedAt?: number;
//     //   explorerLink?: string;
//     // };

//     [Serializable]
//     public class TransactionDetails
//     {
//         [JsonProperty("transactionHash")]
//         public string TransactionHash { get; set; }

//         [JsonProperty("token")]
//         public SwapToken Token { get; set; }

//         [JsonProperty("amountWei")]
//         public string AmountWei { get; set; }

//         [JsonProperty("amountUSDCents")]
//         public BigInteger AmountUSDCents { get; set; }

//         [JsonProperty("completedAt", NullValueHandling = NullValueHandling.Ignore)]
//         public BigInteger CompletedAt { get; set; }

//         [JsonProperty("explorerLink", NullValueHandling = NullValueHandling.Ignore)]
//         public string ExplorerLink { get; set; }
//     }

//     // export type SwapStatusParams = {
//     //   client: ThirdwebClient;
//     //   transactionId: string;
//     //   transactionHash: string;
//     // };

//     [Serializable]
//     public class SwapStatusParams
//     {
//         [JsonProperty("transactionId")]
//         public string TransactionId { get; set; }

//         [JsonProperty("transactionHash")]
//         public string TransactionHash { get; set; }
//     }

//     // export type SwapStatus = {
//     //   transactionId: string;
//     //   transactionType: string;
//     //   source: TransactionDetails;
//     //   destination?: TransactionDetails;
//     //   status: string;
//     //   subStatus: string;
//     //   fromAddress: string;
//     //   toAddress: string;
//     //   failureMessage?: string;
//     //   bridgeUsed?: string;
//     // };

//     [Serializable]
//     public class SwapStatus
//     {
//         [JsonProperty("transactionId")]
//         public string TransactionId { get; set; }

//         [JsonProperty("transactionType")]
//         public string TransactionType { get; set; }

//         [JsonProperty("source")]
//         public TransactionDetails Source { get; set; }

//         [JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
//         public TransactionDetails Destination { get; set; }

//         [JsonProperty("status")]
//         public string Status { get; set; }

//         [JsonProperty("subStatus")]
//         public string SubStatus { get; set; }

//         [JsonProperty("fromAddress")]
//         public string FromAddress { get; set; }

//         [JsonProperty("toAddress")]
//         public string ToAddress { get; set; }

//         [JsonProperty("failureMessage", NullValueHandling = NullValueHandling.Ignore)]
//         public string FailureMessage { get; set; }

//         [JsonProperty("bridgeUsed", NullValueHandling = NullValueHandling.Ignore)]
//         public string BridgeUsed { get; set; }
//     }
// }
