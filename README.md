<p align="center">
<br />
<a href="https://thirdweb.com"><img src="https://github.com/thirdweb-dev/js/blob/main/packages/sdk/logo.svg?raw=true" width="200" alt=""/></a>
<br />
</p>
<h1 align="center">thirdweb Unity SDK</h1>
<p align="center">
<a href="https://discord.gg/thirdweb"><img alt="Join our Discord!" src="https://img.shields.io/discord/834227967404146718.svg?color=7289da&label=discord&logo=discord&style=flat"/></a>

</p>
<p align="center"><strong>Best in class Web3 SDK for Unity games</strong></p>
<br />

# Supported platforms

- [x] WebGL
- [ ] Desktop (coming soon)
- [ ] Android (coming soon)
- [ ] iOS (coming soon)

# Installation

Head over to the [releases](https://github.com/thirdweb-dev/unity-sdk/releases) page and download the latest `.unitypackage` file.

Drag and drop the file into your project.

The package comes with a sample Scene and Prefab examples showcasing the different capabilities of the SDK.

Note: you may need to install the [Newtonsoft.Json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM) package manually if you experience importing errors on different Unity versions.

# Build

- Open your `Build settings`, select `WebGL` as the target platform.
- Open `Player settings` > `Resolution and Presentation` and under `WebGLTemplate` choose `Thirdweb`.
- Save and click `Build and Run` to test out your game in a browser.

If you're uploading your build, set `Compression Format` to `Disabled` in `Player Settings` > `Publishing Settings`.

Note that in order to communicate with the SDK, you need to `Build and run` your project so it runs in a browser context.

#### _**Interacting with the SDK within the Unity Editor is NOT supported.**_

# Usage

```csharp
// instantiate a read only SDK on any EVM chain
var sdk = new ThirdwebSDK("goerli");

// connect the user's wallet - supports Metamask, Coinbase Wallet, WalletConnect and more
var walletAddress = await sdk.wallet.Connect();

// interact with the wallet
CurrencyValue balance = await sdk.wallet.GetBalance();
var signature = await sdk.wallet.Sign("message to sign");

// get an instance of a deployed contract (no ABI required!)
var contract = sdk.GetContract("0x...");

// fetch data from any ERC20/721/1155 or marketplace contract
CurrencyValue currencyValue = await contract.ERC20.TotalSupply();
NFT erc721NFT = await contract.ERC721.Get(tokenId);
List<NFT> erc1155NFTs = await contract.ERC1155.GetAll();
List<Listing> listings = await marketplace.GetAllListings();

// execute transactions from the connected wallet
await contract.ERC20.Mint("1.2");
await contract.ERC721.signature.Mint(signedPayload);
await contract.ERC1155.Claim(tokenId, quantity);
await marketplace.BuyListing(listingId, quantity);

// deploy contracts from the connected wallet
var address = await sdk.deployer.DeployNFTCollection(new NFTContractDeployMetadata {
    name = "My Personal Unity Collection",
    primary_sale_recipient = await sdk.wallet.GetAddress(),
});
```

# Prefabs

The `Examples` folder contains a demo scene using our user-friendly prefabs, check it out!

All Prefabs require the [ThirdwebManager](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/ThirdwebManager.cs) prefab to get the SDK Instance, drag and drop it into your scene and select the networks you want to support from the Inspector.

[Connect Wallet](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_ConnectWallet.cs) - All-in-one drag & drop wallet supporting multiple wallet providers, network switching, balance displaying and more!
- Drag and drop it into your scene and select the wallet providers you want to support from the Inspector.
- You may also choose whether you want to activate the Network Switching feature (leave unchecked if your app only requires one network).

[NFT Loader](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_NFTLoader.cs) - Standalone drag & drop grid/scroll view of NFTs you ask it to display!
- Go to the prefab's Settings in the Inspector.
- Load specific NFTs with token ID.
- Load a specific range of NFTs.
- Load NFTs owned by a specific wallet.
- Or any combination of the above - they will all be displayed automatically in a grid view with vertical scroll!
- Customize the prefab's ScrollView and Content gameobjects if you want your content to behave differently.

[NFT](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_NFT.cs) - Displays an NFT by calling LoadNFT through script!
- Instantiate this Prefab through script.
- Get its Prefab_NFT component.
- Call the LoadNFT function and pass it your NFT struct to display your fetched NFT's images automatically.
- Customize the prefab to add text/decorations and customize LoadNFT to use your NFT's metadata if you want to populate that text.
```csharp
NFT nft = await contract.ERC721.Get(0);
Prefab_NFT nftPrefabScript = Instantiate(nftPrefab);
nftPrefabScript.LoadNFT(nft);
```

[Events](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_Events.cs) - Fetch and manipulate Contract Events with a simple API!
- Get specific events from any contract.
- Get all events from any contract.
- Event listener support with callback actions.
- Optional query filters.

See full documentation on the [thirdweb portal](https://portal.thirdweb.com).
