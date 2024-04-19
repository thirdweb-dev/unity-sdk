<p align="center">
  <br />
  <a href="https://thirdweb.com">
    <img src="https://github.com/thirdweb-dev/js/blob/main/legacy_packages/sdk/logo.svg?raw=true" width="200" alt=""/>
  </a>
  <br />
  <h1 align="center">thirdweb Unity SDK</h1>
  <p align="center">
    <a href="https://discord.gg/thirdweb">
      <img alt="Join our Discord!" src="https://img.shields.io/discord/834227967404146718.svg?color=7289da&label=discord&logo=discord&style=flat"/>
    </a>
    <br />
    <a href="https://thirdweb-dev.github.io/unity-sdk/">
      <img alt="Preview" src="https://img.shields.io/badge/Preview-Unity%20WebGL-brightgreen?logo=unity&style=flat"/>
    </a>
  </p>
</p>

# Documentation

See full documentation on the [thirdweb portal](https://portal.thirdweb.com/unity).

# Technical Demo

Try out our multichain game that leverages Embedded and Smart Wallets to create seamless experiences, built in 3 weeks - [Web3 Warriors](https://web3warriors.thirdweb.com/).

![image](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/171198b2-83e7-4c8a-951b-79126dd47abb)

# Supported platforms

Build games for WebGL, Standalone and Mobile using 1000+ supported chains.

![tw_wallets](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/0c7f5ad1-263e-4cc8-9dfa-97a7f84bc471)

# Installation

Head over to the [releases](https://github.com/thirdweb-dev/unity-sdk/releases) page and download the latest `.unitypackage` file.

Follow our [Getting Started Guide](https://portal.thirdweb.com/unity/getting-started).

All you need is a [ThirdwebManager](https://portal.thirdweb.com/unity/thirdwebmanager) prefab in your scene to interact with the SDK from anywhere!

Various blockchain interaction examples are available in our `Scene_Prefabs` scene.

Payment related interaction examples are available in our `Scene_Pay` scene.

Notes:

- The SDK has been tested on Web, Desktop and Mobile platforms using Unity 2021 and 2022 LTS. We highly recommend using 2022 LTS.
- The example scenes are built using Unity 2022 LTS, it may look off in previous versions of Unity.
- The Newtonsoft DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.
- If using .NET Framework and encountering an error related to HttpUtility, create a file `csc.rsp` that includes `-r:System.Web.dll` and save it under `Assets`.

# Usage

In order to access the SDK, you only need to have a [ThirdwebManager](https://portal.thirdweb.com/unity/thirdwebmanager) in your scene.

```csharp
// Configure the connection
var connection = new WalletConnection(
  provider: WalletProvider.EmbeddedWallet, // The wallet provider you want to connect to (Required)
  chainId: 5,                              // The chain you want to connect to (Required)
  email: "email@email.com"                 // The email you want to authenticate with (Required for this provider)
);

// Connect the wallet
string address = await ThirdwebManager.Instance.SDK.Wallet.Connect(connection);

// Interact with the wallet
CurrencyValue balance = await ThirdwebManager.Instance.SDK.Wallet.GetBalance();
var signature = await ThirdwebManager.Instance.SDK.Wallet.Sign("message to sign");

// Get an instance of a deployed contract (no ABI required!)
var contract = ThirdwebManager.Instance.SDK.GetContract("0x...");

// Fetch data from any ERC20/721/1155 or Marketplace contract
CurrencyValue currencyValue = await contract.ERC20.TotalSupply();
NFT erc721NFT = await contract.ERC721.Get(tokenId);
List<NFT> erc1155NFTs = await contract.ERC1155.GetAll();
List<Listing> listings = await contract.Marketplace.DirectListings.GetAllListings();

// Execute transactions from the connected wallet
await contract.ERC20.Mint("1.2");
await contract.ERC721.Signature.Mint(signedPayload);
await contract.ERC1155.Claim(tokenId, quantity);
await contract.Marketplace.DirectListings.BuyListing(listingId, quantity);

// Custom interactions
var res = await contract.Read<string>("myReadFunction", arg1, arg2, ...);
var txRes = await contract.Write("myWriteFunction", arg1, arg2, ...);
```



# Build

## General

- Use `Smaller (faster) Builds` in the Build Settings (IL2CPP Code Generation in Unity 2022).
- Use IL2CPP over Mono when possible in the Player Settings.
- Using the SDK in the editor (pressing Play) is an accurate reflection of what you can expect to see on native platforms.
- In most cases, setting `Managed Stripping Level` to minimal when using IL2CPP is also helpful - you can find it under `Player Settings` > `Other Settings` > `Optimization`

## WebGL

- In order to communicate with the SDK on WebGL, you need to `Build and run` your project so it runs in a browser context.
- Open your `Build settings`, select `WebGL` as the target platform.
- Open `Player settings` > `Resolution and Presentation` and under `WebGLTemplate` choose `Thirdweb`.
- Save and click `Build and Run` to test out your game in a browser.

Important: If you're uploading your build, set `Compression Format` to `Disabled` in `Player Settings` > `Publishing Settings`.

Please note that Embedded Wallets (OAuth version) may not work when testing locally using Unity's default Build and Run feature for WebGL.

You must host the build or run it locally yourself after adding the `Cross-Origin-Opener-Policy` header and setting it to `same-origin-allow-popups`.

Here's a simple way to do so, assuming you are in your WebGL build output folder:

```csharp
const express = require('express');
const app = express();
const port = 8000;

app.use(function(req, res, next) {
  res.header('Cross-Origin-Opener-Policy', 'same-origin-allow-popups');
  next();
});

app.use(express.static('.'));
app.listen(port, () => console.log(`Server running on http://localhost:${port}`));
```

Once again, please note that no action is needed for hosted builds.

## Mobile

- For Android, it is best to run Force Resolve from the `Assets` menu > `External Dependency Manager` > `Android Resolver` > `Force Resolve` before building your game.
- For iOS, if you are missing a MetaMask package, you can double click on `main.unitypackage` under `Assets\Thirdweb\Plugins\MetaMask\Installer\Packages` and reimport the `iOS` folder (only).
- ~~If you are having trouble building in XCode, make sure `ENABLE_BITCODE` is disabled and that the `Embedded Frameworks` in your `Build Phases` contain potentially missing frameworks like `MetaMask` or `Starscream`. You may also need to remove the `Thirdweb/Core/Plugins/MetaMask/Plugins/iOS/iphoneos/MetaMask_iOS.framework/Frameworks` folder in some cases.~~ Recent versions should no longer require this.

## Miscellaneous
If you don't want to use Minimal Stripping, you could instead create a `link.xml` file under your Assets folder and include assemblies that must be preserved, for instance:
```xml
<linker>
    <!--Thirdweb-->
    <assembly fullname="Amazon.Extensions.CognitoAuthentication" preserve="all" />
    <assembly fullname="AWSSDK.CognitoIdentity" preserve="all" />
    <assembly fullname="AWSSDK.CognitoIdentityProvider" preserve="all" />
    <assembly fullname="AWSSDK.Core" preserve="all" />
    <assembly fullname="AWSSDK.Lambda" preserve="all" />
    <assembly fullname="AWSSDK.SecurityToken" preserve="all" />
    <assembly fullname="embedded-wallet" preserve="all" />

    <!--Other-->
    <assembly fullname="System.Runtime.Serialization" preserve="all" />
    <assembly fullname="Newtonsoft.Json" preserve="all" />
    <assembly fullname="System" preserve="all">
        <type fullname="System.ComponentModel.TypeConverter" preserve="all" />
        <type fullname="System.ComponentModel.ArrayConverter" preserve="all" />
        <type fullname="System.ComponentModel.BaseNumberConverter" preserve="all" />
        <type fullname="System.ComponentModel.BooleanConverter" preserve="all" />
        <type fullname="System.ComponentModel.ByteConverter" preserve="all" />
        <type fullname="System.ComponentModel.CharConverter" preserve="all" />
        <type fullname="System.ComponentModel.CollectionConverter" preserve="all" />
        <type fullname="System.ComponentModel.ComponentConverter" preserve="all" />
        <type fullname="System.ComponentModel.CultureInfoConverter" preserve="all" />
        <type fullname="System.ComponentModel.DateTimeConverter" preserve="all" />
        <type fullname="System.ComponentModel.DecimalConverter" preserve="all" />
        <type fullname="System.ComponentModel.DoubleConverter" preserve="all" />
        <type fullname="System.ComponentModel.EnumConverter" preserve="all" />
        <type fullname="System.ComponentModel.ExpandableObjectConverter" preserve="all" />
        <type fullname="System.ComponentModel.Int16Converter" preserve="all" />
        <type fullname="System.ComponentModel.Int32Converter" preserve="all" />
        <type fullname="System.ComponentModel.Int64Converter" preserve="all" />
        <type fullname="System.ComponentModel.NullableConverter" preserve="all" />
        <type fullname="System.ComponentModel.SByteConverter" preserve="all" />
        <type fullname="System.ComponentModel.SingleConverter" preserve="all" />
        <type fullname="System.ComponentModel.StringConverter" preserve="all" />
        <type fullname="System.ComponentModel.TimeSpanConverter" preserve="all" />
        <type fullname="System.ComponentModel.UInt16Converter" preserve="all" />
        <type fullname="System.ComponentModel.UInt32Converter" preserve="all" />
        <type fullname="System.ComponentModel.UInt64Converter" preserve="all" />
    </assembly>
    <assembly fullname="Nethereum.ABI" preserve="all" />
    <assembly fullname="Nethereum.Accounts" preserve="all" />
    <assembly fullname="Nethereum.BlockchainProcessing" preserve="all" />
    <assembly fullname="Nethereum.Contracts" preserve="all" />
    <assembly fullname="Nethereum.HdWallet" preserve="all" />
    <assembly fullname="Nethereum.Hex" preserve="all" />
    <assembly fullname="Nethereum.JsonRpc.Client" preserve="all" />
    <assembly fullname="Nethereum.JsonRpc.RpcClient" preserve="all" />
    <assembly fullname="Nethereum.Keystore" preserve="all" />
    <assembly fullname="Nethereum.Merkle" preserve="all" />
    <assembly fullname="Nethereum.Merkle.Patricia" preserve="all" />
    <assembly fullname="Nethereum.Model" preserve="all" />
    <assembly fullname="Nethereum.RLP" preserve=" all" />
    <assembly fullname="Nethereum.RPC" preserve=" all" />
    <assembly fullname="Nethereum.Signer" preserve=" all" />
    <assembly fullname="Nethereum.Signer.EIP712" preserve=" all" />
    <assembly fullname="Nethereum.Siwe" preserve=" all" />
    <assembly fullname="Nethereum.Siwe.Core" preserve=" all" />
    <assembly fullname="Nethereum.Util" preserve=" all" />
    <assembly fullname="Nethereum.Web3" preserve=" all" />
</linker>
```

# Additional Links

- [Documentation](https://portal.thirdweb.com/unity)
- [Starter Template: Take Flight!](https://github.com/thirdweb-example/take-flight)
- [Starter Template: Blockventure](https://github.com/thirdweb-example/blockventure)
- [Example Template: Enhancing Unity IAP with Blockchain Interactions](https://blog.thirdweb.com/guides/enhancing-unity-iap-with-blockchain-interactions/)
- [Example Template: Pioneer](https://github.com/thirdweb-example/pioneer)

# Prefab Examples

![image](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/a213b668-0273-400f-a6c1-92a582a35535)

The `Examples` folder contains a demo scene `Scene_Prefabs` using our user-friendly prefabs - they include script examples to get inspired and are entirely optional.

All Prefabs require the [ThirdwebManager](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Core/Scripts/ThirdwebManager.cs) prefab to get the SDK Instance, drag and drop it into your scene and select the networks you want to support from the Inspector.

[Connect Wallet](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_ConnectWallet.cs) - All-in-one drag & drop wallet supporting multiple wallet providers, network switching, balance displaying and more!

- Drag and drop it into your scene.
- Set up the networks you want to support from the ThirdwebManager prefab.
- You can add listeners from the inspector for various wallet events.

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
- Message Signing.

[Smart Wallet](https://github.com/thirdweb-dev/unity-sdk/blob/main/Assets/Thirdweb/Examples/Scripts/Prefabs/Prefab_SmartWallet.cs)

- Adding admins to your smart wallet.
- Removing admins from your smart wallet.
- Creating session keys to grant temporary/restricted access to additional signers.

See full documentation on the [thirdweb portal](https://portal.thirdweb.com/unity).

# Contributing to thirdweb Unity SDK

We warmly welcome contributions to the thirdweb Unity SDK! If you're looking to contribute, here's how you can get started.

## How to Contribute

1. Fork the Repository: Click the "Fork" button at the top right of this page to create your own copy of the repository.
2. Clone Your Fork: Clone your fork to your local machine for development.
3. Create a Feature Branch: Make a new branch for your changes. This helps keep contributions organized.
4. Make Your Changes: Work on your changes. Make sure they are well-tested and don't break existing functionality.
5. Commit Your Changes: Commit your changes with a clear and descriptive commit message.
6. Push to Your Fork: Push your changes to your forked repository.
7. Submit a Pull Request: From your fork, submit a pull request to our main repository. Provide a clear description of your changes and any relevant issue numbers.

Notes:

- For WebGL-specific contributions, you may contribute to our [JS](https://github.com/thirdweb-dev/js/) package as well. The bulk of WebGL-specific behavior goes through its Unity bridge.
- For new Wallet Provider contributions, see our guide to [Submit your Wallet](https://portal.thirdweb.com/unity/wallets/submission).

## Guidelines

- Keep It Simple: Try to keep your contributions small and simple. This makes them easier to review and merge.
- Supported Platforms: Make sure your changes work on either WebGL only, Native platforms only, or both. A good test is to build `Scene_Prefabs` to test your changes there.
- Test Your Code: Ensure your code works as expected and doesn't introduce new issues.
- Be Respectful: When discussing changes, always be respectful and constructive.

# Need Help?

If you're unsure about something or need help, feel free to reach out to us on [Discord](https://discord.gg/thirdweb).

Thank you for contributing to the thirdweb Unity SDK!
