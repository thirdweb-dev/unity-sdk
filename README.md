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
<p align="center"><strong><a href="https://bafybeibh73sdxugia63nvuzxeqrcozalbhszaojcpgaatuli3skou3d3bq.ipfs.dweb.link/">Live Demo</a></strong></p>
<br />

# Supported platforms

![providers-unity](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/8bac2226-a67a-4709-8a5d-49fadd58160c)

More on wallets [here](https://thirdweb.com/dashboard/wallet).

# Installation

Head over to the [releases](https://github.com/thirdweb-dev/unity-sdk/releases) page and download the latest `.unitypackage` file.

Drag and drop the file into your project.

The package comes with a sample Scene and Prefab examples showcasing the different capabilities of the SDK.

All you need is a ThirdwebManager component in your scene. See [documentation](https://portal.thirdweb.com/unity) for more information.

Note: The Newtonsoft DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.

# Build

## WebGL

- Open your `Build settings`, select `WebGL` as the target platform.
- Open `Player settings` > `Resolution and Presentation` and under `WebGLTemplate` choose `Thirdweb`.
- Use `Smaller (faster) Builds` in the Build Settings.
- Save and click `Build and Run` to test out your game in a browser.

If you're uploading your build, set `Compression Format` to `Disabled` in `Player Settings` > `Publishing Settings`.

Note that in order to communicate with the SDK on WebGL, you need to `Build and run` your project so it runs in a browser context.

## Other Platforms

- Use IL2CPP over Mono when possible in the Player Settings.
- Using the SDK in the editor (pressing Play) is an accurate reflection of what you can expect to see on native platforms.
- If building to mobile and running into RPC issues, it is best to run Force Resolve from the `Assets` menu > `External Dependency Manager` > `Android Resolver` > `Force Resolve` for example.
- If building for iOS and missing a Metamask package, you can double click on `main.unitypackage` under `Assets\Thirdweb\Plugins\MetaMask\Installer\Packages` and reimport the `iOS` folder
- In some cases, setting `Managed Stripping Level` to minimal when using IL2CPP is also helpful - you can find it under `Player Settings` > `Other Settings` > `Optimization`

# Usage

In order to access the SDK, you only need to have a [ThirdwebManager](https://portal.thirdweb.com/unity/thirdwebmanager) in your scene.

```csharp
// Reference to your Thirdweb SDK
var sdk = ThirdwebManager.Instance.SDK;

// Configure the connection
var connection = new WalletConnection(
  provider: WalletProvider.Paper,          // The wallet provider you want to connect to (Required)
  chainId: 1,                              // The chain you want to connect to (Required)
  email: "email@email.com"                 // The email you want to authenticate with (Required for this provider)
);

// Connect the wallet
string address = await sdk.wallet.Connect(connection);

// Interact with the wallet
CurrencyValue balance = await sdk.wallet.GetBalance();
var signature = await sdk.wallet.Sign("message to sign");

// Get an instance of a deployed contract (no ABI required!)
var contract = sdk.GetContract("0x...");

// Fetch data from any ERC20/721/1155 or marketplace contract
CurrencyValue currencyValue = await contract.ERC20.TotalSupply();
NFT erc721NFT = await contract.ERC721.Get(tokenId);
List<NFT> erc1155NFTs = await contract.ERC1155.GetAll();
List<Listing> listings = await marketplace.GetAllListings();

// Execute transactions from the connected wallet
await contract.ERC20.Mint("1.2");
await contract.ERC721.signature.Mint(signedPayload);
await contract.ERC1155.Claim(tokenId, quantity);
await marketplace.BuyListing(listingId, quantity);

// Custom interactions
var res = await contract.Read<string>("myReadFunction", arg1, arg2, ...);
var txRes = await contract.Write("myWriteFunction", arg1, arg2, ...);
```

# Prefabs

The `Examples` folder contains a demo scene using our user-friendly prefabs, check it out!

All Prefabs require the [ThirdwebManager](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Core/Scripts/ThirdwebManager.cs) prefab to get the SDK Instance, drag and drop it into your scene and select the networks you want to support from the Inspector.

[Connect Wallet](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_ConnectWallet.cs) - All-in-one drag & drop wallet supporting multiple wallet providers, network switching, balance displaying and more!

- Drag and drop it into your scene.
- Set up the networks you want to support from the ThirdwebManager prefab.
- You can add callbacks from the inspector for when the wallet is connected, disconnected, fails to connect or disconnect, as well as callbacks when the network is switched or fails to do so.

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

[Reading](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_Reading.cs) - Reading from a contract!

- Fetch ERC20 Token(s).
- Fetch ERC721 NFT(s).
- Fetch ERC1155 NFT(s).
- Fetch Marketplace Listing(s).
- Fetch Pack contents.

[Writing](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_Writing.cs) - Writing to a contract!

- Mint ERC20 Token(s).
- Mint ERC721 NFT(s).
- Mint ERC1155 NFT(s).
- Buy Marketplace Listing(s).
- Buy a Pack.

[Miscellaneous](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_Miscellaneous.cs) - More examples!

- Get (Native) Balance.
- Custom Contract Read/Write Calls.
- Authentication.
- Deployment.

See full documentation on the [thirdweb portal](https://portal.thirdweb.com).
