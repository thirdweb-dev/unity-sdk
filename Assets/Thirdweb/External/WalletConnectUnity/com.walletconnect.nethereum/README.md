# WalletConnectUnity Nethereum

This Unity package provides a simple way to integrate WalletConnect with Nethereum.

## Usage

```csharp
// Nethereum's Web3 instance
var web3 = new Web3();

// Instance of WalletConnect singleton
var walletConnect = WalletConnect.Instance;

// Interceptor that will route requests requiring signing to the wallet connected with WalletConnect
var walletConnectUnityInterceptor = new WalletConnectUnityInterceptor(walletConnect);

// Assign the interceptor to the Web3 instance
web3.Client.OverridingRequestInterceptor = walletConnectUnityInterceptor;

// Use the Web3 instance as usual
// This `personal_sign` request will be routed to the wallet connected with WalletConnect
var encodedMessage = new HexUTF8String("Hello WalletConnect!");
var result = await web3.Eth.AccountSigning.PersonalSign.SendRequestAsync(encodedMessage);
```