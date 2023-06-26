# WalletConnectSharp

WalletConnectSharp is an implementation of the [WalletConnect](https://walletconnect.org/) protocol v2 using .NET. This library implements the [WalletConnect Technical Specification](https://docs.walletconnect.org/tech-spec) in .NET to allow C# dApps makers and wallet makers to add support for the open [WalletConnect](https://walletconnect.org/) protocol.

## Installation

install via Nuget

```jsx
dotnet add package WalletConnect.Sign
```

## Usage

### **Dapp Usage**

First you must setup `SignClientOptions` which stores both the `ProjectId` and `Metadata`. You may also optionally specify the storage module to use. By default, the `FileSystemStorage` module is used if none is specified.

```csharp
var dappOptions = new SignClientOptions()
{
    ProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c",
    Metadata = new Metadata()
    {
        Description = "An example dapp to showcase WalletConnectSharpv2",
        Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
        Name = "WalletConnectSharpv2 Dapp Example",
        Url = "https://walletconnect.com"
    },
    // Uncomment to disable persistant storage
    // Storage = new InMemoryStorage()
};
```

Then, you must setup the `ConnectOptions` which define what blockchain, RPC methods and events your dapp will use.

*C# Constructor*

```csharp
var dappConnectOptions = new ConnectOptions()
{
    RequiredNamespaces = new RequiredNamespaces()
    {
        {
            "eip155", new RequiredNamespace()
            {
                Methods = new[]
                {
                    "eth_sendTransaction",
                    "eth_signTransaction",
                    "eth_sign",
                    "personal_sign",
                    "eth_signTypedData",
                },
                Chains = new[]
                {
                    "eip155:1"
                },
                Events = new[]
                {
                    "chainChanged",
                    "accountsChanged",
                }
            }
        }
    }
};
```

*Builder Functions Style*

```csharp
var dappConnectOptions1 = new ConnectOptions()
    .RequireNamespace("eip155", new RequiredNamespace()
        .WithMethod("eth_sendTransaction")
        .WithMethod("eth_signTransaction")
        .WithMethod("eth_sign")
        .WithMethod("personal_sign")
        .WithMethod("eth_signTypedData")
        .WithChain("eip155:1")
        .WithEvent("chainChanged")
        .WithEvent("accountsChanged")
    );
```

With both options defined, you can initialize and connect the SDK

```csharp
var dappClient = await WalletConnectSignClient.Init(dappOptions);
var connectData = await dappClient.Connect(dappConnectOptions);
```

You can grab the `Uri` for the connection request from `connectData`

```csharp
ExampleShowQRCode(connectData.Uri);
```

and await for connection approval using the `Approval` Task object

```csharp
Task<SessionData> sessionConnectTask = connectData.Approval;
SessionData sessionData = await sessionConnectTask;

// or
// SessionData sessionData = await connectData.Approval;
```

This `Task` will return the `SessionData` when the session was approved, or throw an exception when the session rquest has either

* Timed out
* Been Rejected

### **Wallet Usage**

First you must setup `SignClientOptions` which stores both the `ProjectId` and `Metadata`. You may also optionally specify the storage module to use. By default, the `FileSystemStorage` module is used if none is specified.

```csharp
var walletOptions = new SignClientOptions()
{
    ProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c",
    Metadata = new Metadata()
    {
        Description = "An example wallet to showcase WalletConnectSharpv2",
        Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
        Name = "WalletConnectSharpv2 Wallet Example",
        Url = "https://walletconnect.com"
    },
    // Uncomment to disable persistant storage
    // Storage = new InMemoryStorage()
};
```

Once you have options defined, you can initialize the SDK

```csharp
var walletClient = await WalletConnectSignClient.Init(walletOptions);
```

Wallets can pair an incoming session using the session's Uri. Pairing a session lets the Wallet obtain the connection proposal which can then be approved or denied.

```csharp
ProposalStruct proposal = await walletClient.Pair(connectData.Uri);
```

The wallet can then approve or reject the proposal using either of the following

```csharp
string addressToConnect = ...;
var approveData = await walletClient.Approve(proposal, addressToConnect);
await approveData.Acknowledged();
```

```csharp
string[] addressesToConnect = ...;
var approveData = await walletClient.Approve(proposal, addressesToConnect);
await approveData.Acknowledged();
```

```csharp
await walletClient.Reject(proposal, "User rejected");
```


## Examples

There are examples and unit tests in the Tests directory. Some examples include

* BiDirectional Communication
* Basic dApp Example
