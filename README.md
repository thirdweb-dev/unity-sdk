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

The package comes with a sample Scene showcasing the different capabilities of the SDK.

# Build

- Open your `Build settings`, select `WebGL` as the target platform.
- Open `Player settings` > `Resolution and Presentation` and under `WebGLTemplate` choose `Thirdweb`.
- Save and click `Build and Run` to test out your game in a browser.

Note that in order to communicate with the SDK, you need to `Build and run` your project so it runs in a browser context.

#### _**Interacting with the SDK within the Unity Editor is NOT supported.**_

# Usage

```csharp
// instantiate a read only SDK on any EVM chain
var sdk = new ThirdwebSDK("goerli");

// connect a wallet via browser extension
var walletAddress = await sdk.wallet.Connect();

// interact with the wallet
CurrencyValue balance = await sdk.wallet.GetBalance();
var signature = await sdk.wallet.Sign("message to sign");

// get an instance of a deployed contract (no ABI requried!)
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

See full documentation on the [thirdweb portal](https://portal.thirdweb.com).
