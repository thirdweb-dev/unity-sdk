![net-banner](https://github.com/thirdweb-dev/thirdweb-dotnet/assets/43042585/6abcdae9-b49f-492a-98de-b01756e21798)

[<img alt="NuGet Version" src="https://img.shields.io/nuget/v/Thirdweb?logo=nuget&style=for-the-badge" height="30">](https://www.nuget.org/packages/Thirdweb)
[<img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/Thirdweb?logo=nuget&style=for-the-badge" height="30">](https://www.nuget.org/packages/Thirdweb)
[<img alt="Codecov" src="https://img.shields.io/codecov/c/github/thirdweb-dev/thirdweb-dotnet?logo=codecov&style=for-the-badge" height="30">](https://app.codecov.io/gh/thirdweb-dev/thirdweb-dotnet)


## Overview

The Thirdweb .NET SDK is a comprehensive library that allows developers to interact with the blockchain using the .NET framework. It simplifies the integration of Web3 functionality into your .NET applications with a robust set of methods and classes and a minimal amount of dependencies.



## Features

- **Connect to any EVM network:** Easily connect to Ethereum and other EVM-compatible networks.
- **Query blockchain data:** Use Thirdweb RPC to fetch blockchain data efficiently.
- **Interact with smart contracts:** Simplified read and write operations for smart contracts, with various out-of-the-box extensions provided.
- **In-App Wallets:** Integrate user-friendly wallets within your applications, supporting email, phone, and OAuth login.
- **Account Abstraction:** Simplify complex account management tasks with smart wallets.
- **Gasless Transactions:** Enable transactions without requiring users to pay gas fees.
- **Storage Solutions:** Download and upload files using IPFS.
- **Transaction Builder:** Easily build and send transactions.
- **Session Keys:** Advanced control for smart wallets to manage permissions and session durations.
- **Thirdweb Pay:** Easily integrate fiat onramps and cross-chain crypto purchases.
- **Unity Compatibility**: This SDK has been tested successfully in Unity 2022.3+ (Standalone, Mobile and WebGL).
- **Godot Compatibility**: This SDK has been tested successfully in [Godot .NET](https://portal.thirdweb.com/dotnet/godot)

## Installation

To use the Thirdweb .NET SDK in your project, you can either download the source code and build it manually, or install it via NuGet package manager.

Run the following command to install:

```
dotnet add package Thirdweb
```

## Usage

You can access the full documentation at https://portal.thirdweb.com/dotnet

Full API reference also available [here](https://thirdweb-dev.github.io/thirdweb-dotnet/).

### Getting Started

Initialize the Thirdweb client to connect to the blockchain.

For frontend applications:

```csharp
var client = ThirdwebClient.Create(clientId: "myClientId", bundleId: "com.my.bundleid");
```

For backend applications:

```csharp
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
var client = ThirdwebClient.Create(secretKey: secretKey);
```

### Interacting with Smart Contracts

You can interact with smart contracts by creating a contract instance and calling read/write methods.

**Reading Data**

```csharp
var contract = await ThirdwebContract.Create(client: client, address: "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", chain: 421614);
var readResult = await ThirdwebContract.Read<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");
```

**Writing Data**

```csharp
var writeResult = await ThirdwebContract.Write(smartWallet, contract, "mintTo", 0, await smartWallet.GetAddress(), 100);
Console.WriteLine($"Contract write result: {writeResult}");
```

**Using Extensions**

Thirdweb comes with very handy prebuilt extensions so you don't have to rely on dynamic parameters, available for any contract object.

```csharp
// ERC20 balanceOf
var balance = await contract.ERC20_BalanceOf(ownerAddress: "0xOwner");

// DropERC20 (Thirdweb Prebuilt Contract) claim
var claimTx = await contract.DropERC20_Claim(wallet: privateKeyWallet, receiverAddress: "0xReceiver", amount: "1.5");

// Miscellaneous
var nativeContractBalance = await contract.GetBalance(); // Can also take in ERC20 address
var nfts = await contract.ERC721_GetAllNFTs(); // Fetches all NFTs of a contract
var nftImageBytes = await nfts[0].GetNFTImageBytes(client); // NFT type extension to get image bytes
```

Extensions exist for various common standards, thirdweb-specific prebuilt contracts and much more!

### Wallet Interactions

#### In-App Wallets

In-app wallets facilitate user authentication and transactions with support for email, phone, and OAuth logins.

**Email Login**

```csharp
var inAppWallet = await InAppWallet.Create(client: client, email: "email@example.com");

if (!await inAppWallet.IsConnected()) {
    await inAppWallet.SendOTP();
    Console.WriteLine("Please submit the OTP.");
    var otp = Console.ReadLine();
    (var inAppWalletAddress, var canRetry) = await inAppWallet.SubmitOTP(otp);
    if (inAppWalletAddress == null && canRetry) {
        Console.WriteLine("Please submit the OTP again.");
        otp = Console.ReadLine();
        (inAppWalletAddress, _) = await inAppWallet.SubmitOTP(otp);
    }
    if (inAppWalletAddress == null) {
        Console.WriteLine("OTP login failed. Please try again.");
        return;
    }
}

Console.WriteLine($"InAppWallet: {await inAppWallet.GetAddress()}");
```

**OAuth Login**

```csharp
var inAppWallet = await InAppWallet.Create(client, oauthProvider: OAuthProvider.Google);

// Windows console app example
var address = await inAppWallet.LoginWithOauth(
    isMobile: false,
    browserOpenAction: (url) =>
    {
        var psi = new ProcessStartInfo { FileName = url, UseShellExecute = true };
        _ = Process.Start(psi);
    },
);

// Godot standalone example
var address = await ThirdwebManager.Instance.InAppWallet.LoginWithOauth(
        isMobile: OS.GetName() == "Android" || OS.GetName() == "iOS",
        browserOpenAction: (url) => OS.ShellOpen(url),
        mobileRedirectScheme: "thirdweb://"
);
```

#### Smart Wallets

Smart wallets offer advanced functionalities such as gasless transactions and session keys.

**Creating a Smart Wallet**

```csharp
var smartWallet = await SmartWallet.Create(client: client, personalWallet: inAppWallet, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);

Console.WriteLine($"Smart Wallet: {await smartWallet.GetAddress()}");
```

**Gasless Transactions**

```csharp
var writeResult = await ThirdwebContract.Write(smartWallet, contract, "mintTo", 0, await smartWallet.GetAddress(), 100);
Console.WriteLine($"Gasless transaction result: {writeResult}");
```

**Session Key Creation**

Session keys provide temporary keys for smart wallets with specific permissions and durations. This is useful for granting limited access to a wallet.

```csharp
var sessionKey = await smartWallet.CreateSessionKey(
    signerAddress: await privateKeyWallet.GetAddress(),
    approvedTargets: new List<string>() { Constants.ADDRESS_ZERO },
    nativeTokenLimitPerTransactionInWei: "0",
    permissionStartTimestamp: "0",
    permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
    reqValidityStartTimestamp: "0",
    reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
);
```

You may then connect to a specific smart wallet address by passing an account override.

```csharp
var smartWallet = await SmartWallet.Create(...same parameters with new signer, accountAddressOverride: "0xInitialSmartWalletAddress");
```

#### Using Private Key Wallets

Private key wallets allow you to interact with the blockchain using a private key. This is useful for server-side applications.

```csharp
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");
var privateKeyWallet = await PrivateKeyWallet.Create(client: client, privateKeyHex: privateKey);
Console.WriteLine($"PrivateKey Wallet: {await privateKeyWallet.GetAddress()}");

// or generate a private key wallet
var generatedPrivateKeyWallet = await PrivateKeyWallet.Generate(client);
```

### Advanced Features

**RPC Direct Access**

Directly interact with the blockchain using the RPC instance. This allows for low-level access to blockchain data and functions.

```csharp
var rpc = ThirdwebRPC.GetRpcInstance(client, 421614);
var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
Console.WriteLine($"Block number: {blockNumber}");
```

**ZkSync Native Account Abstraction**

ZkSync 0x71 (113) type transactions are supported through the Transaction Builder (DIY) or Smart Wallets (Managed).

**DIY Approach**

```csharp
var tx = await ThirdwebTransaction.Create(
    client: client,
    wallet: privateKeyWallet,
    txInput: new ThirdwebTransactionInput()
    {
        From = await privateKeyWallet.GetAddress(),
        To = await privateKeyWallet.GetAddress(),
        Value = new HexBigInteger(BigInteger.Zero),
    },
    chainId: 300
);
tx.SetZkSyncOptions(
    new ZkSyncOptions(
        paymaster: "0xMyGaslessPaymaster",
        paymasterInput: "0x8c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000"
    )
);
var txHash = await ThirdwebTransaction.Send(transaction: tx);
Console.WriteLine($"Transaction hash: {txHash}");
```

**Managed Approach**

With ZkSync, you don't need to pass an account factory address, and the rest works the same.

```csharp
var zkSyncWallet = await SmartWallet.Create(client: client, personalWallet: inAppWallet, gasless: true, chainId: 300);

Console.WriteLine($"ZkSync Smart Wallet: {await zkSyncWallet.GetAddress()}");

var zkSyncWriteResult = await ThirdwebContract.Write(zkSyncWallet, contract, "mintTo", 0, await zkSyncWallet.GetAddress(), 100);
Console.WriteLine($"ZkSync gasless transaction result: {zkSyncWriteResult}");
```

**Storage Solutions**

Download and upload files using IPFS. This is useful for decentralized storage solutions.

```csharp
var downloadResult = await ThirdwebStorage.Download<string>(client: client, uri: "ipfs://exampleUri");
Console.WriteLine($"Download result: {downloadResult}");

var uploadResult = await ThirdwebStorage.Upload(client: client, path: "path/to/file");
Console.WriteLine($"Upload result preview: {uploadResult.PreviewUrl}");
```

### Thirdweb Pay

Easily integrate fiat onramps and cross-chain crypto purchases.

**Buy With Crypto**

```csharp
// Swap Polygon MATIC to Base ETH
var swapQuoteParams = new BuyWithCryptoQuoteParams(
    fromAddress: walletAddress,
    fromChainId: 137,
    fromTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS,
    toTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS,
    toChainId: 8453,
    toAmount: "0.1"
);
var swapQuote = await ThirdwebPay.GetBuyWithCryptoQuote(client, swapQuoteParams);
Console.WriteLine($"Swap quote: {JsonConvert.SerializeObject(swapQuote, Formatting.Indented)}");

// Initiate swap
var txHash = await ThirdwebPay.BuyWithCrypto(wallet: privateKeyWallet, buyWithCryptoQuote: swapQuote);
Console.WriteLine($"Swap transaction hash: {txHash}");

// Poll for status
var currentSwapStatus = SwapStatus.NONE;
while (currentSwapStatus is not SwapStatus.COMPLETED and not SwapStatus.FAILED)
{
    var swapStatus = await ThirdwebPay.GetBuyWithCryptoStatus(client, txHash);
    currentSwapStatus = Enum.Parse<SwapStatus>(swapStatus.Status);
    Console.WriteLine($"Swap status: {JsonConvert.SerializeObject(swapStatus, Formatting.Indented)}");
    await Task.Delay(5000);
}
```

**Buy With Fiat**

```csharp
// Find out more about supported FIAT currencies
var supportedCurrencies = await ThirdwebPay.GetBuyWithFiatCurrencies(client);
Console.WriteLine($"Supported currencies: {JsonConvert.SerializeObject(supportedCurrencies, Formatting.Indented)}");

// Get a Buy with Fiat quote
var fiatQuoteParams = new BuyWithFiatQuoteParams(
    fromCurrencySymbol: "USD", 
    toAddress: walletAddress, 
    toChainId: "137", 
    toTokenAddress: Thirdweb.Constants.NATIVE_TOKEN_ADDRESS, 
    toAmount: "20"
);
var fiatOnrampQuote = await ThirdwebPay.GetBuyWithFiatQuote(client, fiatQuoteParams);
Console.WriteLine($"Fiat onramp quote: {JsonConvert.SerializeObject(fiatOnrampQuote, Formatting.Indented)}");

// Get a Buy with Fiat link
var onRampLink = ThirdwebPay.BuyWithFiat(fiatOnrampQuote);
Console.WriteLine($"Fiat onramp link: {onRampLink}");

// Open onramp link to start the process (use your framework's version of this)
var psi = new ProcessStartInfo { FileName = onRampLink, UseShellExecute = true };
_ = Process.Start(psi);

// Poll for status
var currentOnRampStatus = OnRampStatus.NONE;
while (currentOnRampStatus is not OnRampStatus.ON_RAMP_TRANSFER_COMPLETED and not OnRampStatus.ON_RAMP_TRANSFER_FAILED)
{
    var onRampStatus = await ThirdwebPay.GetBuyWithFiatStatus(client, fiatOnrampQuote.IntentId);
    currentOnRampStatus = Enum.Parse<OnRampStatus>(onRampStatus.Status);
    Console.WriteLine($"Fiat onramp status: {JsonConvert.SerializeObject(onRampStatus, Formatting.Indented)}");
    await Task.Delay(5000);
}
```

For more information, please refer to the [official documentation](https://portal.thirdweb.com/dotnet).
