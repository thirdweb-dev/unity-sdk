using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Deploy contracts to the blockchain.
    /// </summary>
    public class Deployer : Routable
    {

        public Deployer() : base($"sdk{subSeparator}deployer")
        {
        }

        public async Task<string> DeployNFTCollection(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployNFTCollection"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployNFTDrop(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployNFTDrop"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeploySignatureDrop(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deploySignatureDrop"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployMultiwrap(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployMultiwrap"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployEdition(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployEdition"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployEditionDrop(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployEditionDrop"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployPack(NFTContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployPack"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployToken(TokenContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployToken"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployTokenDrop(TokenContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployTokenDrop"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployMarketplace(MarketplaceContractDeployMetadata metadata)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployMarketplace"), Utils.ToJsonStringArray(metadata));
        }

        public async Task<string> DeployReleasedContract(string releaserAddress, string contractName, object[] constructorParams)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployReleasedContract"), Utils.ToJsonStringArray(releaserAddress, contractName, constructorParams));
        }

        public async Task<string> DeployFromContractUri(string uri, object[] constructorParams)
        {
            return await Bridge.InvokeRoute<string>(getRoute("deployContractFromUri"), Utils.ToJsonStringArray(uri, constructorParams));
        }
    }

    [System.Serializable]
    public struct NFTContractDeployMetadata
    {
        public string name;
        public string description;
        public string image;
        public string external_link;
        public string symbol;
        public string[] trusted_forwarders;
        public string primary_sale_recipient;
        public string fee_recipient;
        public int seller_fee_basis_points;
        public string platform_fee_recipient;
        public int platform_fee_basis_points;
    }

    [System.Serializable]
    public struct TokenContractDeployMetadata
    {
        public string name;
        public string description;
        public string image;
        public string external_link;
        public string symbol;
        public string[] trusted_forwarders;
        public string primary_sale_recipient;
        public string platform_fee_recipient;
        public string platform_fee_basis_points;
    }

    [System.Serializable]
    public struct MarketplaceContractDeployMetadata
    {
        public string name;
        public string description;
        public string image;
        public string external_link;
        public string[] trusted_forwarders;
        public string platform_fee_recipient;
        public string platform_fee_basis_points;
    }
}