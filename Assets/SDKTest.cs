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

    public async void OnButtonClick()
    {
        Debug.Log("Button clicked");
        count++;
        NFT result = await this.contract.ERC721.GetNFT(count.ToString());
        Debug.Log("name: " + result.metadata.name);
        Debug.Log("owner: " + result.owner);
    }
}
