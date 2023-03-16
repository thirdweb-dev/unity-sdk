// using WalletConnectSharp.Core.Models;
// using WalletConnectSharp.Core.Models.Ethereum.Types;

// namespace WalletConnectUnity.Demo.Scripts
// {
//     public static class DemoSignTypedData
//     {
//         public class GasData
//         {
//             [EvmType("uint256")]
//             public string gasLimit;

//             [EvmType("uint256")]
//             public string gasPrice;

//             [EvmType("uint256")]
//             public string baseRelayFee;

//             [EvmType("uint256")]
//             public string pctRelayFee;
//         }

//         public class RelayData
//         {
//             [EvmType("address")]
//             public string senderAddress;

//             [EvmType("uint256")]
//             public string senderNonce;

//             [EvmType("address")]
//             public string relayWorker;

//             [EvmType("address")]
//             public string paymaster;
//         }

//         public class RelayRequest
//         {
//             public string target;

//             public string encodedFunction;

//             public GasData gasData;

//             public RelayData relayData;
//         }

//         public static RelayRequest ExampleData = new RelayRequest()
//         {
//             target = "0x9cf40ef3d1622efe270fe6fe720585b4be4eeeff",
//             encodedFunction =
//                 "0xa9059cbb0000000000000000000000002e0d94754b348d208d64d52d78bcd443afa9fa520000000000000000000000000000000000000000000000000000000000000007",
//             gasData = new GasData()
//             {
//                 gasLimit = "39507",
//                 gasPrice = "1700000000",
//                 pctRelayFee = "70",
//                 baseRelayFee = "0"
//             },
//             relayData = new RelayData()
//             {
//                 senderAddress = "0x22d491bde2303f2f43325b2108d26f1eaba1e32b",
//                 senderNonce = "3",
//                 relayWorker = "0x3baee457ad824c94bd3953183d725847d023a2cf",
//                 paymaster = "0x957F270d45e9Ceca5c5af2b49f1b5dC1Abb0421c"
//             }
//         };

//         public static EIP712Domain Eip712Domain = new EIP712Domain(
//             "GSN Relayed Transaction", "1",
//             42, "0x6453D37248Ab2C16eBd1A8f782a2CBC65860E60B");
//     }
// }
