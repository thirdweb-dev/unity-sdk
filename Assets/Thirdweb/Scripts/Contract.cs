using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Convenient wrapper to interact with any EVM contract
    /// </summary>
    public class Contract
    {
        public string chain;
        public string address;
        public string abi;
        /// <summary>
        /// Call any ERC20 supported functions
        /// </summary>
        public ERC20 ERC20;
        /// <summary>
        /// Call any ERC721 supported functions
        /// </summary>
        public ERC721 ERC721;
        /// <summary>
        /// Call any ERC1155 supported functions
        /// </summary>
        public ERC1155 ERC1155;
        /// <summary>
        /// Call any Marketplace supported functions
        /// </summary>
        public Marketplace marketplace;

        public Contract(string chain, string address, string abi = null) {
            this.chain = chain;
            this.address = address;
            this.abi = abi;
            this.ERC20 = new ERC20(chain, address);
            this.ERC721 = new ERC721(chain, address);
            this.ERC1155 = new ERC1155(chain, address);
            this.marketplace = new Marketplace(chain, address);
        }

        /// <summary>
        /// Read data from a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">The function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The data deserialized to the given typed</returns>
        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<T>(getRoute("call"), argsEncoded);
        }

        /// <summary>
        /// Execute a write transaction on a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">The function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The transaction receipt</returns>
        public async Task<TransactionResult> Write(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), argsEncoded);
        }

        private string getRoute(string functionPath) {
            if (abi != null) {
                return this.address + "#" + abi + "." + functionPath;
            } else {
                return this.address + "." + functionPath;
            }
        }
    }
}