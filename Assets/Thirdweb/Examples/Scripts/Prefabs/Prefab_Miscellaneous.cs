using UnityEngine;
using Thirdweb;

public class Prefab_Miscellaneous : MonoBehaviour
{
    public async void GetBalance()
    {
        try
        {
            CurrencyValue balance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            Debugger.Instance.Log("[Get Balance] Native Balance", balance.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Get Balance] Error", e.Message);
        }
    }

    public async void CustomCall()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x62Cf5485B6C24b707E47C5E0FB2EAe7EbE18EC4c", MY_CUSTOM_CONTRACT_ABI);

            string uri = await contract.Read<string>("uri", 0);
            Debugger.Instance.Log("[Custom Call] Read Custom URI Successful", uri);

            // TransactionResult transactionResult = await contract.Write("claimKitten");
            // Debugger.Instance.Log("[Custom Call] Write Successful", transactionResult.ToString());

            // With Transaction Overrides:
            // await contract.Write("claim", new TransactionRequest
            // {
            //     value = "0.05".ToWei() // 0.05 ETH
            // }, "0xE79ee09bD47F4F5381dbbACaCff2040f2FbC5803", 0, 1);
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Custom Call] Error", e.Message);
        }
    }

    public async void Authenticate()
    {
        try
        {
            // Generate and sign
            LoginPayload data = await ThirdwebManager.Instance.SDK.wallet.Authenticate("example.com");
            // Verify
            string resultAddressOrError = await ThirdwebManager.Instance.SDK.wallet.Verify(data);
            if (await ThirdwebManager.Instance.SDK.wallet.GetAddress() == resultAddressOrError)
            {
                Debugger.Instance.Log("[Authenticate] Successful", resultAddressOrError);
            }
            else
            {
                Debugger.Instance.Log("[Authenticate] Invalid", resultAddressOrError);
            }
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Authenticate] Error", e.Message);
        }
    }

    public async void Deploy()
    {
        try
        {
            string address = await ThirdwebManager.Instance.SDK.deployer.DeployNFTCollection(
                new NFTContractDeployMetadata { name = "Unity Collection", primary_sale_recipient = await ThirdwebManager.Instance.SDK.wallet.GetAddress(), }
            );
            Debugger.Instance.Log("[Deploy] Successful", $"Address: {address}");
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Deploy] Error", e.Message);
        }
    }

    string MY_CUSTOM_CONTRACT_ABI =
        "[{\"inputs\":[{\"internalType\":\"string\",\"name\":\"_name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_symbol\",\"type\":\"string\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"_approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"string\",\"name\":\"prevURI\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"newURI\",\"type\":\"string\"}],\"name\":\"ContractURIUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newRoyaltyRecipient\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newRoyaltyBps\",\"type\":\"uint256\"}],\"name\":\"DefaultRoyalty\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"level\",\"type\":\"uint256\"}],\"name\":\"LevelUp\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"attacker\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"victim\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"level\",\"type\":\"uint256\"}],\"name\":\"Miaowed\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"prevOwner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"OwnerUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"royaltyRecipient\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"royaltyBps\",\"type\":\"uint256\"}],\"name\":\"RoyaltyForToken\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"startTokenId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"endTokenId\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"string\",\"name\":\"baseURI\",\"type\":\"string\"},{\"indexed\":false,\"internalType\":\"bytes\",\"name\":\"encryptedBaseURI\",\"type\":\"bytes\"}],\"name\":\"TokensLazyMinted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"_ids\",\"type\":\"uint256[]\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"_values\",\"type\":\"uint256[]\"}],\"name\":\"TransferBatch\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"_value\",\"type\":\"uint256\"}],\"name\":\"TransferSingle\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"string\",\"name\":\"_value\",\"type\":\"string\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"URI\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"victim\",\"type\":\"address\"}],\"name\":\"attack\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"accounts\",\"type\":\"address[]\"},{\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"}],\"name\":\"balanceOfBatch\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"burn\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_owner\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"_tokenIds\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"_amounts\",\"type\":\"uint256[]\"}],\"name\":\"burnBatch\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_receiver\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_quantity\",\"type\":\"uint256\"}],\"name\":\"claim\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"claimKitten\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"contractURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getBaseURICount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_index\",\"type\":\"uint256\"}],\"name\":\"getBatchIdAtIndex\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getDefaultRoyaltyInfo\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint16\",\"name\":\"\",\"type\":\"uint16\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_tokenId\",\"type\":\"uint256\"}],\"name\":\"getRoyaltyInfoForToken\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint16\",\"name\":\"\",\"type\":\"uint16\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"isGamePaused\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_amount\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_baseURIForTokens\",\"type\":\"string\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"lazyMint\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"batchId\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes[]\",\"name\":\"data\",\"type\":\"bytes[]\"}],\"name\":\"multicall\",\"outputs\":[{\"internalType\":\"bytes[]\",\"name\":\"results\",\"type\":\"bytes[]\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"nextTokenIdToMint\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"salePrice\",\"type\":\"uint256\"}],\"name\":\"royaltyInfo\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"royaltyAmount\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"amounts\",\"type\":\"uint256[]\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeBatchTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"}],\"name\":\"setContractURI\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_royaltyRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_royaltyBps\",\"type\":\"uint256\"}],\"name\":\"setDefaultRoyaltyInfo\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_newOwner\",\"type\":\"address\"}],\"name\":\"setOwner\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_tokenId\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"_recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_bps\",\"type\":\"uint256\"}],\"name\":\"setRoyaltyInfoForToken\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"startGame\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"stopGame\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_tokenId\",\"type\":\"uint256\"}],\"name\":\"uri\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_claimer\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_tokenId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_quantity\",\"type\":\"uint256\"}],\"name\":\"verifyClaim\",\"outputs\":[],\"stateMutability\":\"view\",\"type\":\"function\"}]";
}
