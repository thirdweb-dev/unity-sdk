# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.1.6] - 2024-05-09

### Added

- visionOS support

### Changed

- Upgraded WalletConnectSharp to [v2.3.6](https://github.com/WalletConnect/WalletConnectSharp/releases/tag/v2.3.6)
- Don't use PlayerPrefs for storage in the Editor when targeting WebGL

### [3.1.5] - 2024-04-26

### Added

- `ApplicationFocus` event in UnityEventsDispatcher

### Changed

- Disable reference validation of external DLLs

## [3.1.4] - 2024-04-17

### Changed

- Upgraded WalletConnectSharp to [v2.3.5](https://github.com/WalletConnect/WalletConnectSharp/releases/tag/v2.3.5)

## [3.1.3] - 2024-04-16

### Changed

- Upgraded WalletConnectSharp to [v2.3.4](https://github.com/WalletConnect/WalletConnectSharp/releases/tag/v2.3.4)
- Use Recent Wallet’s redirect when session doesn't have native redirect
- Improve WebSocket reconnection when not having an internet connection from the start

## [3.1.2] - 2024-03-29

### Added

- Dispose WalletConnect on `ApplicationQuit`. This will also dispose the instance when exiting play mode in the Editor.

### Changed

- Don’t pause WebSocket when app loses focus

### Fixed

- Device type detection on Android

## [3.1.1] - 2024-03-25

### Added

- Add `ActiveChainIdChanged` event to `IWalletConnect` interface

### Fixed

- Broken WebGL build

## [3.1.0] - 2024-03-21

### Added

- Types for common EVM methods (e.g. `eth_sendTransaction`, `personal_sign`, etc.)
- Ethereum chain switching with `wallet_switchEthereumChain` and `wallet_addEthereumChain`
- Chain switch event
- Optional `chainID` parameter in `IWalletConnect.RequestAsync` method
- Chain types and constants
- Tests for Core package
- `RelayUrl` customisation

### Changed

- Upgraded WalletConnectSharp to [v2.3.0](https://github.com/WalletConnect/WalletConnectSharp/releases/tag/v2.3.0)
- Disposability of `WebSocket`
- Disable `OrientationTracker` on platforms other than mobile
- Handle more JSON deserialize errors caused by storage corruption

### Fixed

- Session proposal via deep linking didn't work with some wallets
- `com.unity.nuget.newtonsoft-json` dependency version
- Incompatibility with Firebase and Jetifier in Android builds 
