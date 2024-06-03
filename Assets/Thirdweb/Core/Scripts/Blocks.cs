using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public class Blocks
    {
        private readonly ThirdwebSDK _sdk;

        public Blocks(ThirdwebSDK sdk)
        {
            _sdk = sdk;
        }

        /// <summary>
        /// Returns the latest block number
        /// </summary>
        public async Task<BigInteger> GetLatestBlockNumber()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetLatestBlockNumber();
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                var hex = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return hex.Value;
            }
        }

        /// <summary>
        /// Returns the latest block timestamp
        /// </summary>
        public async Task<BigInteger> GetLatestBlockTimestamp()
        {
            var block = await GetBlock(await GetLatestBlockNumber());
            return block.Timestamp.Value;
        }

        /// <summary>
        /// Returns the latest block (with transaction hashes)
        /// </summary>
        /// <param name="blockNumber">Number of the block to retrieve</param>
        public async Task<BlockWithTransactionHashes> GetBlock(BigInteger blockNumber)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetBlock(blockNumber);
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                return await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            }
        }

        /// <summary>
        /// Returns the latest block with transaction data
        /// </summary>
        /// <param name="blockNumber">Number of the block to retrieve</param>
        public async Task<BlockWithTransactions> GetBlockWithTransactions(BigInteger blockNumber)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetBlockWithTransactions(blockNumber);
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                return await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            }
        }
    }
}
