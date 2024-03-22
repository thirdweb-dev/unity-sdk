using UnityEngine;
using System.Collections.Generic;

namespace Thirdweb.Examples
{
    public class Prefab_Writing : MonoBehaviour
    {
        private const string TOKEN_ERC20_CONTRACT = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private const string DROP_ERC20_CONTRACT = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";
        private const string TOKEN_ERC721_CONTRACT = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private const string DROP_ERC721_CONTRACT = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private const string TOKEN_ERC1155_CONTRACT = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private const string DROP_ERC1155_CONTRACT = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";
        private const string MARKETPLACE_CONTRACT = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";
        private const string PACK_CONTRACT = "0xE33653ce510Ee767d8824b5EcDeD27125D49889D";

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
                // var receiverAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                // var payload = new ERC20MintPayload(receiverAddress, "3.2");
                // var signedPayload = await contract.ERC20.Signature.Generate(payload);
                // bool isValid = await contract.ERC20.Signature.Verify(signedPayload);
                // if (isValid)
                // {
                //     Debugger.Instance.Log("Sign minting ERC20...", $"Signature: {signedPayload.signature}");
                //     var result = await contract.ERC20.Signature.Mint(signedPayload);
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
                // var receiverAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                // var payload = new ERC721MintPayload(receiverAddress, meta);
                // var signedPayload = await contract.ERC721.Signature.Generate(payload);
                // bool isValid = await contract.ERC721.Signature.Verify(signedPayload);
                // if (isValid)
                // {
                //     Debugger.Instance.Log("Sign minting ERC721...", $"Signature: {signedPayload.signature}");
                //     var result = await contract.ERC721.Signature.Mint(signedPayload);
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
                // var receiverAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                // var payload = new ERC1155MintPayload(receiverAddress, meta, 1000);
                // var signedPayload = await contract.ERC1155.signature.Generate(payload);
                // You can use an existing token ID to signature mint additional supply
                // var payloadWithSupply = new ERC1155MintAdditionalPayload(receiverAddress, "0", 1000);
                // var signedPayload = await contract.ERC1155.Signature.GenerateFromTokenId(payloadWithSupply);
                // bool isValid = await contract.ERC1155.Signature.Verify(signedPayload);
                // if (isValid)
                // {
                //     Debugger.Instance.Log("Sign minting ERC1155...", $"Signature: {signedPayload.signature}");
                //     var result = await contract.ERC1155.Signature.Mint(signedPayload);
                //     Debugger.Instance.Log("[Mint (Signature) ERC1155] Successful", result.ToString());
                // }
                // else
                // {
                //     Debugger.Instance.Log("Signature Invalid", $"Signature: {signedPayload.signature} is invalid!");
                // }

                // Edition Drop - Claiming
                // bool canClaim = await contract.ERC1155.ClaimConditions.CanClaim("0", 1);
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

        public async void OpenPack()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(PACK_CONTRACT);
                Pack pack = contract.Pack;

                // NewPackInput newPackInput = new NewPackInput()
                // {
                //     rewardsPerPack = "1",
                //     packMetadata = new NFTMetadata()
                //     {
                //         description = "Kitty Pack - Contains Kitty NFTs and Tokens!",
                //         image = "ipfs://QmbpciV7R5SSPb6aT9kEBAxoYoXBUsStJkMpxzymV4ZcVc",
                //         name = "My Epic Kitty Pack"
                //     },
                //     erc20Rewards = new List<ERC20Contents>()
                //     {
                //         new ERC20Contents()
                //         {
                //             contractAddress = TOKEN_ERC20_CONTRACT,
                //             quantityPerReward = "1",
                //             totalRewards = "10000"
                //         },
                //         new ERC20Contents()
                //         {
                //             contractAddress = TOKEN_ERC20_CONTRACT,
                //             quantityPerReward = "1",
                //             totalRewards = "10000"
                //         }
                //     },
                //     erc721Rewards = new List<ERC721Contents>()
                //     {
                //         new ERC721Contents() { contractAddress = TOKEN_ERC721_CONTRACT, tokenId = "0", },
                //         new ERC721Contents() { contractAddress = TOKEN_ERC721_CONTRACT, tokenId = "1", },
                //         new ERC721Contents() { contractAddress = TOKEN_ERC721_CONTRACT, tokenId = "2", },
                //         new ERC721Contents() { contractAddress = TOKEN_ERC721_CONTRACT, tokenId = "3", },
                //     },
                //     erc1155Rewards = new List<ERC1155Contents>()
                //     {
                //         new ERC1155Contents()
                //         {
                //             contractAddress = TOKEN_ERC1155_CONTRACT,
                //             tokenId = "4",
                //             quantityPerReward = "10",
                //             totalRewards = "10000"
                //         },
                //         new ERC1155Contents()
                //         {
                //             contractAddress = TOKEN_ERC1155_CONTRACT,
                //             tokenId = "5",
                //             quantityPerReward = "10",
                //             totalRewards = "10000"
                //         }
                //     },
                // };
                // // Make sure you approve tokens first
                // var result = await pack.Create(newPackInput);
                // Debugger.Instance.Log("[Create Pack] Successful", result.ToString());

                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                PackRewards rewards = await pack.Open("0");
                Debugger.Instance.Log("[Open Pack] Successful", rewards.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Open Pack] Error", e.Message);
            }
        }
    }
}
