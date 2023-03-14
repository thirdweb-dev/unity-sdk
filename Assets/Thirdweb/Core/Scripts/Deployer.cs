using System.Linq;
using System.Threading.Tasks;
using Thirdweb.Contracts.DropERC721;
using Thirdweb.Contracts.DropERC721.ContractDefinition;
using UnityEngine;

namespace Thirdweb
{
    /// <summary>
    /// Deploy contracts to the blockchain.
    /// </summary>
    public class Deployer : Routable
    {
        public Deployer()
            : base($"sdk{subSeparator}deployer") { }

        public async Task<string> DeployNFTCollection(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployNFTCollection"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployNFTDrop(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployNFTDrop"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");

                var deploymentMessage = new DropERC721Deployment();
                var deploymentHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractDeploymentHandler<DropERC721Deployment>();
                var deploymentReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);
                DropERC721Service dropERC721Service = new DropERC721Service(ThirdwebManager.Instance.SDK.nativeSession.web3, deploymentReceipt.ContractAddress);
                var initializeReceipt = await dropERC721Service.InitializeRequestAndWaitForReceiptAsync(
                    defaultAdmin: await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                    name: metadata.name,
                    symbol: metadata.symbol,
                    contractURI: null,
                    trustedForwarders: metadata.trusted_forwarders.ToList(),
                    saleRecipient: metadata.fee_recipient,
                    royaltyRecipient: metadata.primary_sale_recipient,
                    royaltyBps: metadata.seller_fee_basis_points,
                    platformFeeBps: metadata.platform_fee_basis_points,
                    platformFeeRecipient: metadata.platform_fee_recipient
                );
                return deploymentReceipt.ContractAddress;
            }
        }

        public async Task<string> DeploySignatureDrop(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deploySignatureDrop"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployMultiwrap(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployMultiwrap"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployEdition(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployEdition"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployEditionDrop(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployEditionDrop"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployPack(NFTContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployPack"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployToken(TokenContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployToken"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployTokenDrop(TokenContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployTokenDrop"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployMarketplace(MarketplaceContractDeployMetadata metadata)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployMarketplace"), Utils.ToJsonStringArray(metadata));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployReleasedContract(string releaserAddress, string contractName, object[] constructorParams)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployReleasedContract"), Utils.ToJsonStringArray(releaserAddress, contractName, constructorParams));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> DeployFromContractUri(string uri, object[] constructorParams)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("deployContractFromUri"), Utils.ToJsonStringArray(uri, constructorParams));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
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
