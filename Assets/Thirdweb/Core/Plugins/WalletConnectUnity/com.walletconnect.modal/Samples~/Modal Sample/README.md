# WalletConnect Modal Sample

> A sample dApp demonstrating how to select a network, use the modal, sign messages and transactions, and disconnect
> sessions.

## Supported Platforms

- Android, iOS
- Windows, macOS
- WebGL ([read this first](https://github.com/WalletConnect/WalletConnectUnity#webgl-usage))

## Prerequisites

- Unity 2021.3 or above
- IL2CPP code stripping level: Minimal (or lower)
- Project created in [WalletConnect Cloud](https://cloud.walletconnect.com)

## How to Use

0. Instal WalletConnectUnity Modal package. If installing as Git URL, also install UI and Core packages.
1. Import the sample from the Modal package.
2. Fill in the Project ID and Metadata fields in the `Assets/WalletConnectUnity/Resources/WalletConnectProjectConfig` asset.
   - If you don’t have a Project ID, you can create one at [WalletConnect Cloud](https://cloud.walletconnect.com).
   - The `Redirect` fields are optional. They are used to redirect the user back to your app after they approve or reject the session.
3. Run `Modal Sample/Scenes/WalletConnectModal Sample.unity` scene.
