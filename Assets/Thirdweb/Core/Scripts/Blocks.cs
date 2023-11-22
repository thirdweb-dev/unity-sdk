using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public static class Blocks
    {
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

        public static async Task<BigInteger> GetLatestBlockTimestamp()
        {
            var block = await GetBlock(await GetLatestBlockNumber());
            return block.Timestamp.Value;
        }

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
