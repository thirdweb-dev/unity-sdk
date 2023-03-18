using UnityEngine;
using Thirdweb;
using System.Collections.Generic;

public class Prefab_Writing : MonoBehaviour
{
    private const string TOKEN_ERC20_CONTRACT = "0x76Ec89310842DBD9d0AcA3B2E27858E85cdE595A";
    private const string DROP_ERC20_CONTRACT = "0x450b943729Ddba196Ab58b589Cea545551DF71CC";
    private const string TOKEN_ERC721_CONTRACT = "0x45c498Dfc0b4126605DD91eB1850fd6b5BCe9efC";
    private const string DROP_ERC721_CONTRACT = "0x8ED1C3618d70785d23E5fdE767058FA6cA6D9E43";
    private const string TOKEN_ERC1155_CONTRACT = "0x82c488a1BC64ab3b91B927380cca9Db7bF347879";
    private const string DROP_ERC1155_CONTRACT = "0x408308c85D7073192deEAcC1703E234A783fFfF1";
    private const string MARKETPLACE_CONTRACT = "0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A";
    private const string PACK_CONTRACT = "0xC04104DE55dEC5d63542f7ADCf8171278942048E";

    public async void MintERC20()
    {
        try
        {
            // Traditional Minting (Requires Minter Role)
            // Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC20_CONTRACT);

            // Minting
            // var transactionResult = await contract.ERC20.Mint("1.2");
            // Debugger.Instance.Log("[Mint ERC20] Successful", transactionResult.ToString());

            // Signature Minting
            // var receiverAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            // var payload = new ERC20MintPayload(receiverAddress, "3.2");
            // var signedPayload = await contract.ERC20.signature.Generate(payload);
            // bool isValid = await contract.ERC20.signature.Verify(signedPayload);
            // if (isValid)
            // {
            //     Debugger.Instance.Log("Sign minting ERC20...", $"Signature: {signedPayload.signature}");
            //     var result = await contract.ERC20.signature.Mint(signedPayload);
            //     Debugger.Instance.Log("[Mint (Signature) ERC20] Successful", result.ToString());
            // }
            // else
            // {
            //     Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
            // }

            // Claiming
            Debugger.Instance.Log("Request Sent", "Pending confirmation...");
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(DROP_ERC20_CONTRACT);
            var result = await contract.ERC20.Claim("0.3");
            Debugger.Instance.Log("[Claim ERC20] Successful", result.ToString());
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
            // Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC721_CONTRACT);

            // NFTMetadata meta = new NFTMetadata()
            // {
            //     name = "Unity NFT",
            //     description = "Minted From Unity",
            //     image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc",
            // };

            // Minting
            // var result = await contract.ERC721.Mint(meta);
            // Debugger.Instance.Log("[Mint ERC721] Successful", result.ToString());

            // Signature Minting
            // var receiverAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            // var payload = new ERC721MintPayload(receiverAddress, meta);
            // var signedPayload = await contract.ERC721.signature.Generate(payload);
            // bool isValid = await contract.ERC721.signature.Verify(signedPayload);
            // if (isValid)
            // {
            //     Debugger.Instance.Log("Sign minting ERC721...", $"Signature: {signedPayload.signature}");
            //     var result = await contract.ERC721.signature.Mint(signedPayload);
            //     Debugger.Instance.Log("[Mint (Signature) ERC721] Successful", result.ToString());
            // }
            // else
            // {
            //     Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
            // }

            // NFT Drop Claiming
            Debugger.Instance.Log("Request Sent", "Pending confirmation...");
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(DROP_ERC721_CONTRACT);
            var result = await contract.ERC721.Claim(1);
            Debugger.Instance.Log("[Claim ERC721] Successful", result[0].ToString());
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
            // Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC1155_CONTRACT);

            // NFTMetadata meta = new NFTMetadata()
            // {
            //     name = "Unity NFT",
            //     description = "Minted From Unity",
            //     image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc",
            // };

            // Minting
            // var result = await contract.ERC1155.Mint(new NFTMetadataWithSupply() { supply = 10, metadata = meta });
            // Debugger.Instance.Log("[Mint ERC1155] Successful", result.ToString());
            // You can use an existing token ID to mint additional supply
            // var result = await contract.ERC1155.MintAdditionalSupply("0", 10);
            // Debugger.Instance.Log("[Mint Additional Supply ERC1155] Successful", result.ToString());

            // Signature Minting
            // var receiverAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            // var payload = new ERC1155MintPayload(receiverAddress, meta, 1000);
            // var signedPayload = await contract.ERC1155.signature.Generate(payload);
            // You can use an existing token ID to signature mint additional supply
            // var payloadWithSupply = new ERC1155MintAdditionalPayload(receiverAddress, "0", 1000);
            // var signedPayload = await contract.ERC1155.signature.GenerateFromTokenId(payloadWithSupply);
            // bool isValid = await contract.ERC1155.signature.Verify(signedPayload);
            // if (isValid)
            // {
            //     Debugger.Instance.Log("Sign minting ERC1155...", $"Signature: {signedPayload.signature}");
            //     var result = await contract.ERC1155.signature.Mint(signedPayload);
            //     Debugger.Instance.Log("[Mint (Signature) ERC1155] Successful", result.ToString());
            // }
            // else
            // {
            //     Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
            // }

            // Edition Drop - Claiming
            // bool canClaim = await contract.ERC1155.claimConditions.CanClaim("0", 1);
            // if (!canClaim)
            // {
            //     Debugger.Instance.Log("[Mint ERC1155] Cannot Claim", "Connected wallet not eligible to claim.");
            //     return;
            // }

            // Edition Drop Claiming
            Debugger.Instance.Log("Request Sent", "Pending confirmation...");
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(DROP_ERC1155_CONTRACT);
            TransactionResult transactionResult = await contract.ERC1155.Claim("0", 1);
            Debugger.Instance.Log("[Claim ERC1155] Successful", transactionResult.ToString());

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
            Marketplace marketplace = contract.marketplace;

            var transactionResult = await marketplace.BuyListing("0", 1);
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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(PACK_CONTRACT);
            Pack pack = contract.pack;

            // NewPackInput newPackInput = new NewPackInput()
            // {
            //     rewardsPerPack = "1",
            //     packMetadata = new NFTMetadata()
            //     {
            //         description = "Kitty Pack - Contains Kitty NFTs and Tokens!",
            //         image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc",
            //         name = "My Epic Kitty Pack"
            //     },
            //     erc20Contents = new List<ERC20Contents>()
            //     {
            //         new ERC20Contents()
            //         {
            //             contractAddress = TOKEN_ERC20_CONTRACT,
            //             quantityPerReward = "1",
            //             totalRewards = "100"
            //         }
            //     },
            //     erc721Contents = new List<ERC721Contents>()
            //     {
            //         new ERC721Contents() { contractAddress = TOKEN_ERC721_CONTRACT, tokenId = "11", }
            //     },
            //     erc1155Contents = new List<ERC1155Contents>()
            //     {
            //         new ERC1155Contents()
            //         {
            //             contractAddress = TOKEN_ERC1155_CONTRACT,
            //             tokenId = "3",
            //             quantityPerReward = "1",
            //             totalRewards = "100"
            //         }
            //     },
            // };
            // // Make sure you approve tokens first
            // var result = await pack.Create(newPackInput);
            // Debugger.Instance.Log("[Create Pack] Successful", result.ToString());

            Debugger.Instance.Log("Request Sent", "Pending confirmation...");
            PackRewards rewards = await pack.Open("1");
            Debugger.Instance.Log("[Open Pack] Successful", rewards.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Open Pack] Error", e.Message);
        }
    }
}
