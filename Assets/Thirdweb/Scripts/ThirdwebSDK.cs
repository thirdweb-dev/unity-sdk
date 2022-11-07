using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// The entry point for the thirdweb SDK.
    /// </summary>
    public class ThirdwebSDK
    {
        private string chainOrRPC;
        public ThirdwebSDK(string chainOrRPC) 
        {
            this.chainOrRPC = chainOrRPC;
            Bridge.Initialize(chainOrRPC);
        }

        public Task<string> Connect() 
        {
            return Bridge.Connect();
        }

        public Contract GetContract(string address)
        {
            return new Contract(this.chainOrRPC, address);
        }
    }
}