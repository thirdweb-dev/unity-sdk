using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts;
using Nethereum.ABI.Decoders;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.Services;
using Nethereum.Contracts.ContractHandlers;
using WalletConnectSharp.Unity;
using WalletConnectSharp.Core.Models.Ethereum;

namespace Thirdweb
{
    public class TransactionManager
    {
        public static async Task<TWResult> ThirdwebRead<TWFunction, TWResult>(string contractAddress, TWFunction functionMessage)
            where TWFunction : FunctionMessage, new()
        {
            var queryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractAddress, functionMessage);
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(string contractAddress, TWFunction functionMessage, string weiValue = "0")
            where TWFunction : FunctionMessage, new()
        {
            functionMessage.AmountToSend = BigInteger.Parse(weiValue);

            var transactionHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractTransactionHandler<TWFunction>();
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(contractAddress, functionMessage);
            return receipt.ToTransactionResult();
        }
    }
}
