using UnityEngine;
using Thirdweb;

public class SDKTest : MonoBehaviour
{
    private ThirdwebSDK sdk;
    private Contract contract;
    private int count;
    void Start()
    {
        this.sdk = new ThirdwebSDK("goerli");
        this.contract = sdk.GetContract("0x2e01763fA0e15e07294D74B63cE4b526B321E389");
    }

    public void OnLoginCLick()
    {
        Debug.Log("Login clicked");
        sdk.Connect();
    }

    public async void OnButtonClick()
    {
        Debug.Log("Button clicked");
        count++;
        NFT result = await this.contract.ERC721.GetNFT(count.ToString());
        Debug.Log("name: " + result.metadata.name);
        Debug.Log("owner: " + result.owner);
    }

    public async void OnWriteButtonClick()
    {
        Debug.Log("Claim button clicked");
        count++;
        //var result = await this.contract.ERC721.Transfer("0x2247d5d238d0f9d37184d8332aE0289d1aD9991b", count.ToString());
        var result = await this.contract.ERC721.Claim(1);
        Debug.Log("result id: " + result[0].id);
        Debug.Log("result receipt: " + result[0].receipt.transactionHash);
    }
}
