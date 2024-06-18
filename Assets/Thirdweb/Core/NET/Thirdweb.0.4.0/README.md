# Thirdweb .NET SDK

![NuGet Version](https://img.shields.io/nuget/v/Thirdweb)
[![codecov](https://codecov.io/gh/thirdweb-dev/thirdweb-dotnet/graph/badge.svg?token=AFF179W07C)](https://codecov.io/gh/thirdweb-dev/thirdweb-dotnet)

## Overview

The Thirdweb .NET SDK is a powerful library that allows developers to interact with the blockchain using the .NET framework.
It provides a set of convenient methods and classes to simplify the integration of Web3 functionality into your .NET applications.

## Features

- Connect to any EVM network
- Query blockchain data using Thirdweb RPC
- Interact with smart contracts
- In-App Wallets
- Account Abstraction
- Compatible with GoDot

## Installation

To use the Thirdweb .NET SDK in your project, you can either download the source code and build it manually, or install it via NuGet package manager.

```
dotnet add package Thirdweb
```

## Usage

**Simple Example**

```csharp
// A single client fits most use cases
var client = ThirdwebClient.Create(clientId: "myClientId", bundleId: "com.my.bundleid");

// Optionally pass abi, if not passed we fetch it for you
var contract = await ThirdwebContract.Create(client: client, address: "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", chain: 421614);
var readResult = await ThirdwebContract.Read<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Create InAppWallet wallet as signer to unlock web2 auth
var inAppWallet = await InAppWallet.Create(client: client, email: "email@email.com"); // or email: null, phoneNumber: "+1234567890"

// Relog if InAppWallet wallet not logged in
if (!await inAppWallet.IsConnected())
{
    await inAppWallet.SendOTP();
    Console.WriteLine("Please submit the OTP.");
    var otp = Console.ReadLine();
    (var inAppWalletAddress, var canRetry) = await inAppWallet.SubmitOTP(otp);
    if (inAppWalletAddress == null && canRetry)
    {
        Console.WriteLine("Please submit the OTP again.");
        otp = Console.ReadLine();
        (inAppWalletAddress, _) = await inAppWallet.SubmitOTP(otp);
    }
    if (inAppWalletAddress == null)
    {
        Console.WriteLine("OTP login failed. Please try again.");
        return;
    }
}

// Create a smart wallet to unlock gasless features, with InAppWallet as a signer
var smartWallet = await SmartWallet.Create(client: client, personalWallet: inAppWallet, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);

// Log addresses
Console.WriteLine($"InAppWallet: {await inAppWallet.GetAddress()}");
Console.WriteLine($"Smart Wallet: {await smartWallet.GetAddress()}");

// Sign, triggering deploy as needed and 1271 verification if it's a smart wallet
var message = "Hello, Thirdweb!";
var signature = await smartWallet.PersonalSign(message);
Console.WriteLine($"Signed message: {signature}");

var writeResult = await ThirdwebContract.Write(smartWallet, contract, "mintTo", 0, await smartWallet.GetAddress(), 100);
Console.WriteLine($"Contract write result: {writeResult}");
```

**Advanced Example**

```csharp
using Thirdweb;

// Do not use secret keys client side, use client id/bundle id instead
var secretKey = Environment.GetEnvironmentVariable("THIRDWEB_SECRET_KEY");
// Do not use private keys client side, use InAppWallet/SmartWallet instead
var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");

// Fetch timeout options are optional, default is 60000ms
var client = ThirdwebClient.Create(secretKey: secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 30000, rpc: 60000));

var contract = await ThirdwebContract.Create(client: client, address: "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", chain: 421614);
var readResult = await ThirdwebContract.Read<string>(contract, "name");
Console.WriteLine($"Contract read result: {readResult}");

// Create wallets (this is an advanced use case, typically one wallet is plenty)
var privateKeyWallet = await PrivateKeyWallet.Create(client: client, privateKeyHex: privateKey);
var inAppWallet = await InAppWallet.Create(client: client, email: "email@email.com"); // or email: null, phoneNumber: "+1234567890"

// // Reset InAppWallet (optional step for testing login flow)
// if (await inAppWallet.IsConnected())
// {
//     await inAppWallet.Disconnect();
// }

// Relog if InAppWallet not logged in
if (!await inAppWallet.IsConnected())
{
    await inAppWallet.SendOTP();
    Console.WriteLine("Please submit the OTP.");
    var otp = Console.ReadLine();
    (var inAppWalletAddress, var canRetry) = await inAppWallet.SubmitOTP(otp);
    if (inAppWalletAddress == null && canRetry)
    {
        Console.WriteLine("Please submit the OTP again.");
        otp = Console.ReadLine();
        (inAppWalletAddress, _) = await inAppWallet.SubmitOTP(otp);
    }
    if (inAppWalletAddress == null)
    {
        Console.WriteLine("OTP login failed. Please try again.");
        return;
    }
}

// Create smart wallet with InAppWallet signer
var smartWallet = await SmartWallet.Create(client: client, personalWallet: inAppWallet, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);

// Connect the smart wallet with InAppWallet signer and grant a session key to pk wallet (advanced use case)
_ = await smartWallet.CreateSessionKey(
    signerAddress: await privateKeyWallet.GetAddress(),
    approvedTargets: new List<string>() { Constants.ADDRESS_ZERO },
    nativeTokenLimitPerTransactionInWei: "0",
    permissionStartTimestamp: "0",
    permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
    reqValidityStartTimestamp: "0",
    reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
);

// Reconnect to same smart wallet with pk wallet as signer (specifying wallet address override)
smartWallet = await SmartWallet.Create(
    client: client,
    personalWallet: privateKeyWallet,
    factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
    gasless: true,
    chainId: 421614,
    accountAddressOverride: await smartWallet.GetAddress()
);

// Log addresses
Console.WriteLine($"PrivateKey Wallet: {await privateKeyWallet.GetAddress()}");
Console.WriteLine($"InAppWallet: {await inAppWallet.GetAddress()}");
Console.WriteLine($"Smart Wallet: {await smartWallet.GetAddress()}");

// Sign, triggering deploy as needed and 1271 verification if it's a smart wallet
var message = "Hello, Thirdweb!";
var signature = await smartWallet.PersonalSign(message);
Console.WriteLine($"Signed message: {signature}");

var balanceBefore = await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", await smartWallet.GetAddress());
Console.WriteLine($"Balance before mint: {balanceBefore}");

var writeResult = await ThirdwebContract.Write(smartWallet, contract, "mintTo", 0, await smartWallet.GetAddress(), 100);
Console.WriteLine($"Contract write result: {writeResult}");

var balanceAfter = await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", await smartWallet.GetAddress());
Console.WriteLine($"Balance after mint: {balanceAfter}");

// Storage actions

// // Will download from IPFS or normal urls
// var downloadResult = await ThirdwebStorage.Download<string>(client: client, uri: "AnyUrlIncludingIpfs");
// Console.WriteLine($"Download result: {downloadResult}");

// // Will upload to IPFS
// var uploadResult = await ThirdwebStorage.Upload(client: client, path: "AnyPath");
// Console.WriteLine($"Upload result preview: {uploadResult.PreviewUrl}");


// Access RPC directly if needed, generally not recommended

// var rpc = ThirdwebRPC.GetRpcInstance(client, 421614);
// var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
// Console.WriteLine($"Block number: {blockNumber}");

// Use ZkSync Native AA in 3 steps

// // 1. Prepare a transaction directly, or with Contract.Prepare
// var tx = await ThirdwebTransaction.Create(
//     client: client,
//     wallet: privateKeyWallet,
//     txInput: new ThirdwebTransactionInput()
//     {
//         From = await privateKeyWallet.GetAddress(),
//         To = await privateKeyWallet.GetAddress(),
//         Value = new HexBigInteger(BigInteger.Zero),
//     },
//     chainId: 300
// );
// // 2. Set zkSync options
// tx.SetZkSyncOptions(
//     new ZkSyncOptions(
//         // Paymaster contract address
//         paymaster: "0xMyGaslessPaymaster",
//         // IPaymasterFlow interface encoded data
//         paymasterInput: "0x8c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000"
//     )
// );
// // 3. Send as usual, it's now gasless!
// var txHash = await ThirdwebTransaction.Send(transaction: tx);
// Console.WriteLine($"Transaction hash: {txHash}");

```
