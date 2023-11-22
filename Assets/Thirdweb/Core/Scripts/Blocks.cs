using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public static class Blocks
    {
        /// <summary>
        /// Returns the latest block number
        /// </summary>
        public static async Task<BigInteger> GetLatestBlockNumber()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetLatestBlockNumber();
            }
            else
            {
                var hex = await Utils.GetWeb3().Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return hex.Value;
            }
        }

        /// <summary>
        /// Returns the latest block timestamp
        /// </summary>
        public static async Task<BigInteger> GetLatestBlockTimestamp()
        {
            var block = await GetBlock(await GetLatestBlockNumber());
            return block.Timestamp.Value;
        }

        /// <summary>
        /// Returns the latest block (with transaction hashes)
        /// </summary>
        /// <param name="blockNumber">Number of the block to retrieve</param>
        public static async Task<BlockWithTransactionHashes> GetBlock(BigInteger blockNumber)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetBlock(blockNumber);
            }
            else
            {
                return await Utils.GetWeb3().Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            }
        }

        /// <summary>
        /// Returns the latest block with transaction data
        /// </summary>
        /// <param name="blockNumber">Number of the block to retrieve</param>
        public static async Task<BlockWithTransactions> GetBlockWithTransactions(BigInteger blockNumber)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetBlockWithTransactions(blockNumber);
            }
            else
            {
                return await Utils.GetWeb3().Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
            }
        }
    }
}
