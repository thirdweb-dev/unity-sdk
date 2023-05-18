# magic-unity

Magic empowers developers to protect their users via an innovative, passwordless authentication flow without the UX compromises that burden traditional OAuth implementations. 

[Documentation](https://magic.link/docs/login-methods/email/integration/unity)

## Supported Platform
It's recommended to apply this plugin in Unity 2021.3.3f1 or newer. Developers may build 2d Unity mobile Apps both in iOS and Android. 

WebGL is currently not supported, but you may use [magic-js](https://github.com/magiclabs/magic-js) in a JS context to enable passwordless authentication flow.

## Installation

Install the package by double clicking the `MagicUnity.unitypackage`, or drag it into your project

## Quick Start

Try the prefab in `prefabs/Magic Example.prefab` to get a quick start!

### Instantiate Magic

Create a script with the following code and bind this to a Canvas or object that you love to start with

```c#
public class MagicUnity : MonoBehaviour
{
    // Attach this script when you start the canvas 
    void Start()
    {
        Magic magic = new Magic("YOUR_PUBLISHABLE_KEY");
        
        // Append the instance here, so that it can be shared across the project
        Magic.Instance = magic;
    }
}
```

### User Authentication

```c#
await magic.Auth.LoginWithMagicLink("hiro@magic.link");
```

### Web3 interaction

After the user has been authenticated, now it's a good time to get the users on the blockchain. 

Magic Unity builds on top of Nethereum to enable web3 functionalities. For more detail about Nethereum, please check their official docs https://docs.nethereum.com/en/latest/ 
and their github repo about [RPC payloads](https://github.com/Nethereum/Nethereum/tree/f0f7cbd225fadfce681faff004a57e480428e62b/src/Nethereum.RPC)

```c#
        // Get Eth Account  
         var ethAccounts = new EthAccounts(Magic.Instance.Provider);
        var accounts = await ethAccounts.SendRequestAsync();
         
         // Eth sign
        var personalSign = new EthSign(Magic.Instance.Provider);
        var transactionInput = new TransactionInput{Data = "Hello world"};
        var res = await personalSign.SendRequestAsync(accounts[0], "hello world");
    
        // Send Transaction
        var transaction = new EthSendTransaction(Magic.Instance.Provider);
        var transactionInput = new TransactionInput
            { To = accounts[0], Value = new HexBigInteger(10), From = accounts[0]};
        var hash = await transaction.SendRequestAsync(transactionInput);
```

### DLL not loaded Error
![image](https://user-images.githubusercontent.com/33166884/175986685-6423ffd8-51e2-4251-833b-bdf78fa35fa9.png)

If you find this after loading the package. Please refer to the solution here in our [doc](https://magic.link/docs/login-methods/email/integration/unity#newton-json-version-error) for the incompatible version error

### Support
More blockchain support will be coming soon. Feel free to send your requests and issues to `support@magic.link` or via our helpdesk. Happy coding!

