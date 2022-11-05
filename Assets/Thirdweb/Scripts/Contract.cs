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
    }
}