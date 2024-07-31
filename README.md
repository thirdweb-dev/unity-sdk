![Thirdweb Unity SDK](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/0eb16b66-317b-462b-9eb1-9425c0929c96)

[<img alt="Unity Documentation" src="https://img.shields.io/badge/Unity-Documentation-blue?logo=unity&style=for-the-badge" height="30">](https://portal.thirdweb.com/unity)
[<img alt=".NET Documentation" src="https://img.shields.io/badge/.NET-Documentation-purple?logo=dotnet&style=for-the-badge" height="30">](https://portal.thirdweb.com/dotnet)

<div style="border-left: 4px solid #f0ad4e; padding: 10px; background-color: #fcf8e3;">
  <strong>⚠️ Note: Work in Progress</strong>
  <p>This project is currently under active development. Features, APIs, and behavior may change frequently.</p>
</div>

# Technical Demo

Try out our multichain game that leverages In-App Wallets and Account Abstraction to create seamless experiences, built in 3 weeks - [Web3 Warriors](https://web3warriors.thirdweb.com/).

![image](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/171198b2-83e7-4c8a-951b-79126dd47abb)

# Supported Platforms & Wallets

**Build games for WebGL, Desktop and Mobile using 1000+ supported chains, with various login options!**

|                Wallet Provider                | WebGL     | Desktop    | Mobile    |
| --------------------------------------------- | :-------: | :-------:  | :-------: |
| **In-App Wallet** (Email, Social, Phone)      | ✔️        | ✔️        | ✔️        |
| **Private Key Wallet** (Guest Mode)           | ✔️        | ✔️        | ✔️        |
| **Wallet Connect**                            | ✔️        | ✔️        | ✔️        |
| **MetaMask (Browser Extension)**              | ✔️        | —          | —         |
| **Smart Wallet** (ERC4337)                    | ✔️        | ✔️        | ✔️        |

<sub>✔️ Supported</sub> &nbsp; <sub>❌ Not Supported</sub> &nbsp; <sub>— Not Applicable</sub>

# Why Upgrade to v5?

Thirdweb's Unity SDK v5 takes the approach of taking all its core functionality from our robust [.NET SDK](https://portal.thirdweb.com/dotnet).

It also gets rid of the WebGL Bridge that previously existed, unifying not only APIs, but behaviors and results.

Using the .NET SDK as a core, we unlock that last bit of flexibility that was much needed in v4, and removes all the clutter.

Composability allows you to simply use our SDK anywhere without worrying about the state it is tied to.

Most APIs are chain agnostic, allowing you to create wallets for different chains, interact with contracts on different chains, without having to switch networks or reinitialize the SDK.

Using .NET cross-platform allows for a much more native and predictable experience, and upgrades become a lot less scary to import!

The `ThirdwebManager` is now simply a wrapper simplifying your interaction with the underlying APIs, it preserves familiar APIs from v4:
- `ThirdwebManager.Instance.SDK.GetContract` is now `ThirdwebManager.Instance.GetContract`
- `ThirdwebManager.Instance.SDK.Wallet.Connect` is now `ThirdwebManager.Instance.ConnectWallet`
- The `ThirdwebManager` now can handle multiple wallet connections and tracks the active wallet, you may set it yourself too.
- These functions will return `ThirdwebContract` and `IThirdwebWallet` type objects that have plenty of extensions to play with!

That's really it for the Unity side of things, the rest is all handled internally to bridge the gap between raw .NET and Unity!
- Much cleaner and lighter Unity package.
- The .NET SDK was designed in a composable way while preserving the simple APIs we all know and love.
- Connect to as many wallets as you like in parallel!
- Use different settings for every connection.
- No AWS SDK dependency, Nethereum is now also only used for types, using thirdweb infrastructure is now a much more optimized experience.
- No behavioral differences cross-platform, what you see in the editor is what you get in WebGL, Standalone and Mobile runtime platforms.

# Getting Started

Head over to the [releases](https://github.com/thirdweb-dev/unity-sdk/releases) page and download the latest `.unitypackage` file.

Try out `Scene_Playground` to explore some of the functionality and get onboarded to the SDK nicely!

**Notes:**
- The SDK has been tested on Web, Desktop and Mobile platforms using Unity 2021 and 2022 LTS. We **highly recommend** using 2022 LTS.
- The example scenes are built using Unity 2022 LTS, they may look off in previous versions of Unity.
- The Newtonsoft DLL is included as part of the Unity Package, feel free to deselect it if you already have it installed as a dependency to avoid conflicts.
- If using .NET Framework and encountering an error related to HttpUtility, create a file `csc.rsp` that includes `-r:System.Web.dll` and save it under `Assets`.
- If you have conflicting DLLs from other SDKs, in most cases our SDK will be compatible with previous versions, use version control and test removing duplicates.


# Build Instructions

## General

- Use `Smaller (faster) Builds` in the Build Settings (IL2CPP Code Generation in Unity 2022).
- Use IL2CPP over Mono when possible in the Player Settings.
- Using the SDK in the editor (pressing Play) is an accurate reflection of what you can expect to see on native platforms.
- Set `Managed Stripping Level` to `Minimal` - you can find it under `Player Settings` > `Other Settings` > `Optimization`

## WebGL

- When uploading a final build, set `Compression Format` to `Disabled` in `Player Settings` > `Publishing Settings`.
- To test In-App Wallets (OAuth) you must host the build or run it locally yourself after adding the `Cross-Origin-Opener-Policy` header and setting it to `same-origin-allow-popups`.

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

- Our package comes with EDM4U which resolves dependencies at runtime, you may Force Resolve from the `Assets` menu > `External Dependency Manager` > `Android Resolver` > `Force Resolve` before building your game or edit its settings as you please.
- If using InAppWallet OAuth on mobile, make sure custom schemes matching your bundle id are set in your `Plugins/AndroidManifest.xml` or equivalent. It should be set to your bundle id, for instance `com.thirdweb.unitysdk`


# Need Help?

If you're unsure about something or need help, feel free to reach out though our [Support Portal](https://thirdweb.com/support).

Thank you for trying out the thirdweb Unity SDK!
