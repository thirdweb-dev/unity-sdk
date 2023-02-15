using UnityEngine;
using Thirdweb;


public class Prefab_Writing : MonoBehaviour
{
    public async void MintERC20()
    {
        try
        {
            // Traditional Minting (Requires Minter Role)
            Contract contract = new Contract("goerli", "0xB4870B21f80223696b68798a755478C86ce349bE");

            TransactionResult transactionResult = await contract.ERC20.Mint("1.2");
            Debugger.Instance.Log("[Mint ERC20] Successful", transactionResult.ToString());

            // Signature Minting
            // var payload = new ERC20MintPayload("0xE79ee09bD47F4F5381dbbACaCff2040f2FbC5803", "3.2");
            // var p = await contract.ERC20.signature.Generate(payload);
            // await contract.ERC20.signature.Mint(p);
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Mint ERC20] Error", e.Message);
        }
    }

    public async void MintERC721()
    {
        try
        {
            // NFT Collection Signature Minting
            Contract contract = new Contract("goerli", "0x8bFD00BD1D3A2778BDA12AFddE5E65Cca95082DF");

            NFTMetadata meta = new NFTMetadata()
            {
                name = "Unity NFT",
                description = "Minted From Unity (signature)",
                image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc"
            };
            string connectedAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            ERC721MintPayload payload = new ERC721MintPayload(connectedAddress, meta);
            ERC721SignedPayload signedPayload = await contract.ERC721.signature.Generate(payload); // Typically generated on the backend

            TransactionResult transactionResult = await contract.ERC721.signature.Mint(signedPayload);
            Debugger.Instance.Log("[Mint ERC721] Successful", transactionResult.ToString());

            // NFT Drop Claiming
            // var result = await contract.ERC721.Claim(1);
            // Debug.Log("claimed tokenId: " + result[0].id);
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Mint ERC721] Error", e.Message);
        }
    }

    public async void MintERC1155()
    {
        try
        {
            // Edition Drop Claiming
            Contract contract = new Contract("goerli", "0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A");

            bool canClaim = await contract.ERC1155.claimConditions.CanClaim("0", 1);
            if (!canClaim)
            {
                Debugger.Instance.Log("[Mint ERC721] Cannot Claim", "Connected wallet not eligible to claim.");
                return;
            }

            TransactionResult transactionResult = await contract.ERC1155.Claim("0", 1);
            Debugger.Instance.Log("[Mint ERC1155] Successful", transactionResult.ToString());

            // Edition Drop - Signature minting additional supply
            // var payload = new ERC1155MintAdditionalPayload("0xE79ee09bD47F4F5381dbbACaCff2040f2FbC5803", "1");
            // payload.quantity = 3;
            // var p = await contract.ERC1155.signature.GenerateFromTokenId(payload);
            // var result = await contract.ERC1155.signature.Mint(p);
            // Debug.Log("sigminted tokenId: " + result.id);
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Mint ERC1155] Error", e.Message);
        }
    }

    public async void BuyListing()
    {
        try
        {
            Contract contract = new Contract("goerli", "0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Marketplace marketplace = contract.marketplace;

            TransactionResult transactionResult = await marketplace.BuyListing("0", 1);
            Debugger.Instance.Log("[Buy Listing] Successful", transactionResult.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Buy Listing] Error", e.Message);
        }
    }
}
