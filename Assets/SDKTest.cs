using UnityEngine;
using Thirdweb;
using TMPro;

public class SDKTest : MonoBehaviour
{
    private ThirdwebSDK sdk;
    private int count;
    public TMP_Text loginButton;
    public TMP_Text resultText;

    void Start()
    {
        sdk = new ThirdwebSDK("goerli");
    }

    void Update() {
    }

    public async void OnLoginCLick()
    {
        Debug.Log("Login clicked");
        
        loginButton.text = "Connecting...";
        string address = await sdk.Connect();
        loginButton.text = "Connected as: " + address.Substring(0, 6) + "...";
    }

    public async void GetERC721()
    {
        var contract = sdk.GetContract("0x2e01763fA0e15e07294D74B63cE4b526B321E389"); // NFT Drop
        Debug.Log("Button clicked");
        count++;
        resultText.text = "Fetching Token: " + count;
        NFT result = await contract.ERC721.Get(count.ToString());
        Debug.Log("name: " + result.metadata.name);
        Debug.Log("owner: " + result.owner);
        resultText.text = result.metadata.name;
        // int supply = await contract.ERC721.TotalClaimedSupply();
        // fetchButton.text = supply.ToString();
        // string uri = await contract.Read<string>("tokenURI", count);
        // fetchButton.text = uri;
    }

    public async void GetERC1155()
    {
        var contract = sdk.GetContract("0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A"); // Edition Drop
        Debug.Log("Button clicked");
        count++;
        resultText.text = "Fetching Token: " + count;
        NFT result = await contract.ERC1155.Get(count.ToString());
        Debug.Log("name: " + result.metadata.name);
        Debug.Log("owner: " + result.owner);
        resultText.text = result.metadata.name + " (x" + result.quantityOwned + ")";
        // int supply = await contract.ERC721.TotalClaimedSupply();
        // fetchButton.text = supply.ToString();
        // string uri = await contract.Read<string>("tokenURI", count);
        // fetchButton.text = uri;
    }

    public async void MintERC721()
    {
        var nftCollection = sdk.GetContract("0x8bFD00BD1D3A2778BDA12AFddE5E65Cca95082DF"); // NFT Collection
        Debug.Log("Claim button clicked");
        //var result = await contract.ERC721.Transfer("0x2247d5d238d0f9d37184d8332aE0289d1aD9991b", count.ToString());
        resultText.text = "claiming...";
        // var result = await contract.ERC721.Claim(1);
        // Debug.Log("result id: " + result[0].id);
        // Debug.Log("result receipt: " + result[0].receipt.transactionHash);
        // claimButton.text = "claimed tokenId: " + result[0].id;

        
        var meta = new NFTMetadata();
        meta.name = "Unity NFT";
        meta.description = "Minted From Unity (signature)";
        // get a cute kitten image url
        meta.image = "https://placekitten.com/200/300";
        
        // var result = await nftCollection.ERC721.Mint(meta);
        // claimButton.text = "minted tokenId: " + result.id;

        var payload = new ERC721MintPayload();
        payload.metadata = meta;
        payload.to = "0xE79ee09bD47F4F5381dbbACaCff2040f2FbC5803";
        // TODO allow passing dates as unix timestamps
        var p = await nftCollection.ERC721.signature.Generate(payload);
        Debug.Log("sig:" + p.signature);
        var result = await nftCollection.ERC721.signature.Mint(p);
        resultText.text = "sigminted tokenId: " + result.id;
    }

    public async void MintERC1155()
    {
        var contract = sdk.GetContract("0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A"); // Edition Drop
        Debug.Log("Claim button clicked");
        resultText.text = "claiming...";
        var result = await contract.ERC1155.Claim("0", 1);
        Debug.Log("result receipt: " + result[0].receipt.transactionHash);
        resultText.text = "claim successful";
    }
}
