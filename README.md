![Thirdweb Unity SDK](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/0eb16b66-317b-462b-9eb1-9425c0929c96)

[<img alt="Unity Documentation" src="https://img.shields.io/badge/Unity-Documentation-blue?logo=unity&style=for-the-badge" height="30">](https://portal.thirdweb.com/unity/v5)
[<img alt=".NET Documentation" src="https://img.shields.io/badge/.NET-Documentation-purple?logo=dotnet&style=for-the-badge" height="30">](https://portal.thirdweb.com/dotnet)

# Technical Demo

Experience our multichain game demo leveraging In-App Wallets and Account Abstraction built in three weeks - [Web3 Warriors](https://web3warriors.thirdweb.com/).

![image](https://github.com/thirdweb-dev/unity-sdk/assets/43042585/171198b2-83e7-4c8a-951b-79126dd47abb)

# Supported Platforms & Wallets

**Build games for WebGL, Desktop, and Mobile using 1000+ supported chains, with various login options!**

| Wallet Provider                           | WebGL | Desktop | Mobile |
| ----------------------------------------- | :---: | :-----: | :----: |
| **In-App Wallet** (Email, Phone, Socials) |  ✔️   |   ✔️    |   ✔️   |
| **Ecosystem Wallet** (IAW w/ partner permissions) |  ✔️   |   ✔️    |   ✔️   |
| **Private Key Wallet** (Guest Mode)       |  ✔️   |   ✔️    |   ✔️   |
| **Wallet Connect Wallet** (400+ Wallets)  |  ✔️   |   ✔️    |   ✔️   |
| **MetaMask Wallet** (Browser Extension)   |  ✔️   |    —    |   —    |
| **Smart Wallet** (Account Abstraction)    |  ✔️   |   ✔️    |   ✔️   |

<sub>✔️ Supported</sub> &nbsp; <sub>❌ Not Supported</sub> &nbsp; <sub>— Not Applicable</sub>

# Why Upgrade to v5?

Thirdweb's Unity SDK v5 leverages the robust [.NET SDK](https://portal.thirdweb.com/dotnet) for core functionality, providing a unified API and behavior across platforms.

## Key Improvements:

- **Unified API:** Say goodbye to the WebGL Bridge. Now, enjoy a consistent experience across WebGL, Desktop, and Mobile platforms.
- **Enhanced Composability:** Use our SDK anywhere without worrying about its state. APIs are chain agnostic, enabling seamless interaction with multiple chains.
- **Native Experience:** The .NET core provides a native, predictable experience, making upgrades less daunting.
- **Ecosystem Wallets** The ultimate cross-platform wallet product, suitable for ecosystems wanting to grow out of their shell. Fast, shareable with third parties securely, non-custodial.
- **Simplified `ThirdwebManager`:**
  - `ThirdwebManager.Instance.SDK.GetContract` is now `ThirdwebManager.Instance.GetContract`, returning `ThirdwebContract`.
  - `ThirdwebManager.Instance.SDK.Wallet.Connect` is now `ThirdwebManager.Instance.ConnectWallet`, returning `IThirdwebWallet`.
  - Handles multiple wallet connections and tracks the active wallet.
  - The prefab is now much simpler and straightforward to setup.
- **Optimized Package:** Cleaner, lighter Unity package with minimal dependencies, enhancing performance. We took control of all the layers and rewrote them, further improving the experience when using thirdweb infra. No AWS SDK. Nethereum is used only for types/encoding. Newtonsoft.Json and EDM4U are included.
- **Cross-Platform Consistency:** No behavioral differences between platforms. What you see in the editor is what you get in WebGL, Standalone, and Mobile runtime platforms.

## Note:

To achieve full .NET core functionality for WebGL, we include a modified version of [WebGLThreadingPatcher](https://github.com/VolodymyrBS/WebGLThreadingPatcher) and have adapted our .NET core to work around any potential issues running raw .NET libraries in WebGL, though all tasks will be executed on one thread. This is required due to Unity's lack of support for C# multithreading in WebGL.

# Getting Started

1. **Download:** Head over to the [releases](https://github.com/thirdweb-dev/unity-sdk/releases) page and download the latest `.unitypackage` file.
2. **Explore:** Try out `Scene_Playground` to explore functionality and get onboarded.
3. **Learn:** Explore the [Unity v5 SDK Docs](https://portal.thirdweb.com/unity/v5) and the [.NET SDK Docs](https://portal.thirdweb.com/dotnet) to find a full API reference.

**Notes:**

- Tested on Unity 2021.3+, 2022.3+, Unity 6 Preview. We recommend using 2022 LTS.
- Newtonsoft DLL included; deselect if already installed to avoid conflicts.
- If using .NET Framework and encountering `HttpUtility` errors, create `csc.rsp` with `-r:System.Web.dll` under `Assets`.
- Use version control and test removing duplicate DLLs if conflicts arise.

# Build Instructions

## General

- **Build Settings:** Use `Smaller (faster) Builds` / `Shorter Build Time`.
- **Player Settings:** Use IL2CPP over Mono when available.
- **Stripping Level:** Set `Managed Stripping Level` to `Minimal` (`Player Settings` > `Other Settings` > `Optimization`). (Alternatively, if you do not want to use Minimal Stripping, you may use a linker.xml instead to preserve assemblies that are being stripped and causing errors at runtime)

## WebGL

- **Compression Format:** Set to `Disabled` (`Player Settings` > `Publishing Settings`) for final builds.
- **Testing In-App Wallets (Social Login):** Host the build or run it locally with `Cross-Origin-Opener-Policy` set to `same-origin-allow-popups`.

Example setup for testing In-App Wallet (Social Login) locally:

```javascript
// YourWebGLOutputFolder/server.js
const express = require("express");
const app = express();
const port = 8000;

app.use((req, res, next) => {
  res.header("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
  next();
});

app.use(express.static("."));
app.listen(port, () =>
  console.log(`Server running on http://localhost:${port}`)
);

// run it with `node server.js`
```

No action needed for hosted builds.

## Mobile

- **EDM4U:** Comes with the package, resolves dependencies at runtime. Use `Force Resolve` from `Assets` > `External Dependency Manager` > `Android Resolver`.
- **Redirect Schemes:** Set custom schemes matching your bundle ID in `Plugins/AndroidManifest.xml` or equivalent for InAppWallet OAuth.

# Need Help?

For any questions or support, visit our [Support Portal](https://thirdweb.com/support).

Thank you for trying out the Thirdweb Unity SDK!
