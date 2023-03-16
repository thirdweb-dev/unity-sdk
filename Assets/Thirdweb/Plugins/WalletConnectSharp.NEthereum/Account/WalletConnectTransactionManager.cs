using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Model;
using Nethereum.RLP;
using Nethereum.RPC.Accounts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Mappers;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using Nethereum.Web3.Accounts;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.NEthereum.Model;

namespace WalletConnectSharp.NEthereum.Account
{
    public class WalletConnectTransactionManager : TransactionManager
    {
        public static readonly int[] EIP1559Chains = new[] { 1, };

        public bool SupportsEIP1559
        {
            get { return EIP1559Chains.Contains(_session.ChainId); }
        }

        private WalletConnectSession _session;
        private IAccount _account;
        private bool allowEthSign;

        public WalletConnectTransactionManager(IClient client, WalletConnectSession session, IAccount account, bool allowEthSign)
            : base(client)
        {
            _session = session;
            _account = account;
            this.allowEthSign = allowEthSign;
        }

        public override async Task<string> SignTransactionAsync(TransactionInput transaction)
        {
            if (transaction.ChainId == null)
            {
                transaction.ChainId = new HexBigInteger(new BigInteger(_session.ChainId));
            }

            if (transaction.Nonce == null)
            {
                var nextNonce = await _account.NonceService.GetNextNonceAsync();
                transaction.Nonce = nextNonce;
            }

            if (SupportsEIP1559)
            {
                if (transaction.Type == null)
                {
                    transaction.Type = new HexBigInteger(new BigInteger(TransactionType.EIP1559.AsByte()));
                }
            }
            else
            {
                if (transaction.Type == null)
                {
                    transaction.Type = new HexBigInteger(new BigInteger(TransactionType.LegacyChainTransaction.AsByte()));
                }
            }

            if (transaction.Gas == null)
            {
                var estimatedGas = await EstimateGasAsync(transaction);
                transaction.Gas = estimatedGas;
            }

            if (transaction.GasPrice == null && !SupportsEIP1559)
            {
                var estimatedGasPrice = await GetGasPriceAsync(transaction);
                transaction.GasPrice = estimatedGasPrice;
            }

            await SetTransactionFeesOrPricingAsync(transaction);

            try
            {
                var request = new NEthSignTransaction(transaction);

                var response = await _session.Send<NEthSignTransaction, EthResponse>(request);

                return response.Result;
            }
            catch (WalletException e)
            {
                if (!e.Message.ToLower().Contains("method not supported") || !allowEthSign)
                    throw;

                string hash;
                SignedTransaction signedTx;
                if (!SupportsEIP1559)
                {
                    signedTx = new LegacyTransactionChainId(transaction.To, transaction.Value, transaction.Nonce, transaction.GasPrice, transaction.Gas, transaction.Data, transaction.ChainId);
                    hash = "0x" + signedTx.RawHash.ToHex();
                }
                else
                {
                    signedTx = new Transaction1559(
                        transaction.ChainId,
                        transaction.Nonce,
                        transaction.MaxPriorityFeePerGas,
                        transaction.MaxFeePerGas,
                        transaction.Gas,
                        transaction.To,
                        transaction.Value,
                        transaction.Data,
                        transaction.AccessList.ToSignerAccessListItemArray()
                    );
                    hash = "0x" + signedTx.RawHash.ToHex();
                }

                var request = new EthSign(_account.Address, hash);

                var response = await _session.Send<EthSign, EthResponse>(request);

                var signature = response.Result;

                //Setup some NEthereum signature objects
                var ecdsaSignature = ECDSASignatureFactory.ExtractECDSASignature(signature);
                var ethSignature = EthECDSASignatureFactory.ExtractECDSASignature(signature);

                // Recover EthECKey + Public Key
                var key = EthECKey.RecoverFromSignature(ethSignature, hash.HexToByteArray());
                var pubKey = key.GetPubKey(false);

                // Calculate Rec ID (needed for both EIP1559 and Legacy)
                var recId = -1;

                for (var i = 0; i < 4; i++)
                {
                    var rec = ECKey.RecoverFromSignature(i, ecdsaSignature, signedTx.RawHash, false);
                    if (rec != null)
                    {
                        var k = rec.GetPubKey(false);
                        if (k != null && k.SequenceEqual(pubKey))
                        {
                            recId = i;
                            break;
                        }
                    }
                }

                if (recId == -1)
                    throw new Exception("Could not construct a recoverable key. This should never happen.");

                if (!SupportsEIP1559)
                {
                    //We must sign the legacy transaction
                    //But also update the V value to include
                    //The chainId

                    //Calculate V
                    var v = transaction.ChainId * new BigInteger(2) + recId + 35;

                    //Update signature to use new V
                    ecdsaSignature.V = v.ToBytesForRLPEncoding();
                }
                else
                {
                    //We must sign and calculate Y Parity V
                    ecdsaSignature.V = new[] { (byte)recId };
                }

                signedTx.SetSignature(new EthECDSASignature(ecdsaSignature.R, ecdsaSignature.S, ecdsaSignature.V));

                return "0x" + signedTx.GetRLPEncoded().ToHex();
            }
        }
    }
}
