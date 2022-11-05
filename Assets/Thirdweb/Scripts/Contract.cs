using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb
{

    /// <summary>
    /// Convenient wrapper to interact with any EVM contract
    /// </summary>
    public class Contract
    {
        public string chain;
        public string address;
        public ERC721 ERC721;
        public Contract(string chain, string address) {
            this.chain = chain;
            this.address = address;
            this.ERC721 = new ERC721(chain, address);
        }

        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            toJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<T>(getRoute("call"), argsEncoded);
        }

        public async Task<TransactionResult> Write(string functionName, params object[] args)
        {
            string [] argsEncoded = new string[args.Length + 1];
            argsEncoded[0] = functionName;
            toJsonStringArray(args).CopyTo(argsEncoded, 1);
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), argsEncoded);
        }

        private string getRoute(string functionPath) {
            return this.address + "." + functionPath;
        }

        private string[] toJsonStringArray(params object[] args) {
            string[] stringArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                // if value type, convert to string otherwise serialize to json
                if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs[i] = args[i].ToString();
                }
                else
                {
                    stringArgs[i] = JsonUtility.ToJson(args[i]);
                }
            }
            return stringArgs;
        }
    }
}