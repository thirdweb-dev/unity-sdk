using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Convenient wrapper to interact with any EVM contract
    /// </summary>
    public class Contract : Routable
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
        /// <summary>
        /// Call any Pack supported functions
        /// </summary>
        public Pack pack;
        /// <summary>
        /// Call any Contract Event functions
        /// </summary>
        public Events events;

        public Contract(string chain, string address, string abi = null) : base(abi != null ? $"{address}{Routable.subSeparator}{abi}" : address)
        {
            this.chain = chain;
            this.address = address;
            this.abi = abi;
            this.ERC20 = new ERC20(baseRoute);
            this.ERC721 = new ERC721(baseRoute);
            this.ERC1155 = new ERC1155(baseRoute);
            this.marketplace = new Marketplace(chain, address);
            this.pack = new Pack(chain, address);
            this.events = new Events(baseRoute);
        }

        public async Task<CurrencyValue> GetBalance()
        {
            return await Bridge.InvokeRoute<CurrencyValue>($"sdk{separator}getBalance", Utils.ToJsonStringArray(address));
        }

        /// <summary>
        /// Read data from a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The data deserialized to the given typed</returns>
        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            string[] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<T>(getRoute("call"), argsEncoded);
        }

        /// <summary>
        /// Execute a write transaction on a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The transaction receipt</returns>
        public Task<TransactionResult> Write(string functionName, params object[] args)
        {
            return Write(functionName, null, args);
        }

        /// <summary>
        /// Execute a write transaction on a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="transactionOverrides">Overrides to pass with the transaction</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The transaction receipt</returns>
        public async Task<TransactionResult> Write(string functionName, TransactionRequest? transactionOverrides, params object[] args)
        {
            args = args ?? new object[0];
            var hasOverrides = transactionOverrides != null;
            string[] argsEncoded = new string[args.Length + (hasOverrides ? 2 : 1)];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            if (hasOverrides)
            {
                argsEncoded[argsEncoded.Length - 1] = Utils.ToJson(transactionOverrides);
            }
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), argsEncoded);
        }
    }
}