using UnityEngine;
using Thirdweb;
using System.Collections.Generic;

public class Prefab_Writing : MonoBehaviour
{
    public async void MintERC20()
    {
        try
        {
            // Traditional Minting (Requires Minter Role)
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x76Ec89310842DBD9d0AcA3B2E27858E85cdE595A");

            // Minting
            // var transactionResult = await contract.ERC20.Mint("1.2");
            // Debugger.Instance.Log("[Mint ERC20] Successful", transactionResult.ToString());

            // Signature Minting
            var receiverAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            var payload = new ERC20MintPayload(receiverAddress, "3.2");
            var signedPayload = await contract.ERC20.signature.Generate(payload);
            bool isValid = await contract.ERC20.signature.Verify(signedPayload);
            if (isValid)
            {
                Debugger.Instance.Log("Sign minting...", $"Signature: {signedPayload.signature}");
                var result = await contract.ERC20.signature.Mint(signedPayload);
                Debugger.Instance.Log("[Mint (Signature) ERC20] Successful", result.ToString());
            }
            else
            {
                Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
            }
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
            // NFT Collection Signature Minting (Requires Mint Permission)
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x45c498Dfc0b4126605DD91eB1850fd6b5BCe9efC");

            NFTMetadata meta = new NFTMetadata()
            {
                name = "Unity NFT",
                description = "Minted From Unity",
                image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc",
            };

            // Minting
            // var result = await contract.ERC721.Mint(meta);
            // Debugger.Instance.Log("[Mint ERC721] Successful", result.ToString());

            // Signature Minting
            var receiverAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            var payload = new ERC721MintPayload(receiverAddress, meta);
            var signedPayload = await contract.ERC721.signature.Generate(payload);
            bool isValid = await contract.ERC721.signature.Verify(signedPayload);
            if (isValid)
            {
                Debugger.Instance.Log("Sign minting...", $"Signature: {signedPayload.signature}");
                var result = await contract.ERC721.signature.Mint(signedPayload);
                Debugger.Instance.Log("[Mint (Signature) ERC721] Successful", result.ToString());
            }
            else
            {
                Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
            }

            // NFT Drop Claiming
            // var result = await contract.ERC721.Claim(1);
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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A");

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Marketplace marketplace = contract.marketplace;

            TransactionResult transactionResult = await marketplace.BuyListing("0", 1);
            Debugger.Instance.Log("[Buy Listing] Successful", transactionResult.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Buy Listing] Error", e.Message);
        }
    }

    public async void OpenPack()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Pack pack = contract.pack;

            PackRewards rewards = await pack.Open("0");
            Debugger.Instance.Log("[Open Pack] Successful", rewards.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Open Pack] Error", e.Message);
        }
    }
}
