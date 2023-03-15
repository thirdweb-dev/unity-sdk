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
        public static async Task<TWResult> ThirdwebRead<TWFunction, TWResult>(ContractHandler contractHandler, TWFunction functionMessage)
            where TWFunction : FunctionMessage, new()
        {
            var queryHandler = contractHandler.EthApiContractService.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractHandler.ContractAddress, functionMessage);
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(ContractHandler contractHandler, TWFunction functionMessage, string weiValue = "0")
            where TWFunction : FunctionMessage, new()
        {
            if (Utils.ActiveWalletConnectSession())
            {
                functionMessage.FromAddress = WalletConnect.Instance.Session.Accounts[0];
            }
            functionMessage.AmountToSend = BigInteger.Parse(weiValue);
            var transactionHandler = contractHandler.EthApiContractService.GetContractTransactionHandler<TWFunction>();
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(contractHandler.ContractAddress, functionMessage);
            return receipt.ToTransactionResult();
        }
    }
}
