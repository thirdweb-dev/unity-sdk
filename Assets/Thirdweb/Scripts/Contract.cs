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
        public ERC20 ERC20;
        public ERC721 ERC721;
        public ERC1155 ERC1155;
        public Marketplace marketplace;

        public Contract(string chain, string address) {
            this.chain = chain;
            this.address = address;
            this.ERC20 = new ERC20(chain, address);
            this.ERC721 = new ERC721(chain, address);
            this.ERC1155 = new ERC1155(chain, address);
            this.marketplace = new Marketplace(chain, address);
        }

        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<T>(getRoute("call"), argsEncoded);
        }

        public async Task<TransactionResult> Write(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            Utils.ToJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), argsEncoded);
        }

        private string getRoute(string functionPath) {
            return this.address + "." + functionPath;
        }
    }
}