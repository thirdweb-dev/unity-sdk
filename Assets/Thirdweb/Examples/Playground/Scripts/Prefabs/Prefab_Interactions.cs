using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thirdweb.Pay;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity.Examples
{
    public class Prefab_Interactions : MonoBehaviour
    {
        private readonly string _tokenErc20ContractAddress = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private readonly string _tokenErc721ContractAddress = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private readonly string _tokenErc1155ContractAddress = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private readonly string _dropErc20ContractAddress = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";
        private readonly string _dropErc721ContractAddress = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private readonly string _dropErc1155ContractAddress = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";

        private readonly BigInteger _chainId = 421614;

        private ThirdwebClient _client;
        private IThirdwebWallet _connectedWallet;
        private bool _useConnectedWallet;

        private void Awake()
        {
            // Find the grid in childen, assign any Button_GetWalletBalance to GetWalletBalance for example

            var grid = GetComponentInChildren<GridLayoutGroup>();
            var buttons = grid.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                var buttonName = button.name;
                var methodName = buttonName.Replace("Button_", "");
                var method = GetType().GetMethod(methodName);
                if (method != null)
                {
                    button.onClick.AddListener(() => method.Invoke(this, null));
                }
            }
        }

        private void Start()
        {
            _client = ThirdwebManager.Instance.Client;
        }

        private async Task<IThirdwebWallet> GetSmartWallet()
        {
            var privateKeyWallet = await PrivateKeyWallet.Generate(_client);
            return await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614);
        }

        public void UseConnectedWallet(bool useConnectedWallet)
        {
            _useConnectedWallet = useConnectedWallet;
            _connectedWallet = _useConnectedWallet ? FindObjectOfType<Prefab_ConnectWallet>().ActiveWallet : null;
        }

        #region Common

        public async void GetWalletBalance()
        {
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client); // Generate a new wallet
            var chainId = BigInteger.One;
            var result = await wallet.GetBalance(chainId);
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            var address = await wallet.GetAddress();
            Debugger.Instance.Log("Native Wallet Balance", $"Address: {address}\nChain:{chainId}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }

        public async void GetERC20Balance()
        {
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var chainId = BigInteger.One;
            var tokenAddress = "0x6b175474e89094c44da98b954eedeac495271d0f"; // DAI token address
            var result = await wallet.GetBalance(chainId, tokenAddress);
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            var address = await wallet.GetAddress();
            Debugger.Instance.Log("Token Wallet Balance", $"Address: {address}\nChain:{chainId}\nToken Address: {tokenAddress}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }

        public async void GetContractBalance()
        {
            var contractAddress = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe"; // Ethereum Foundation
            var chainId = BigInteger.One;
            var contract = await ThirdwebContract.Create(_client, contractAddress, chainId);
            var result = await contract.GetBalance(); // this can take in an erc20 token address as well
            var weiBalance = result.ToString();
            var ethBalance = weiBalance.ToEth(decimalsToDisplay: 4, addCommas: true);
            Debugger.Instance.Log("Contract Balance", $"Contract Address: {contractAddress}\nChain:{chainId}\nWei: {weiBalance}\nEth (or equivalent): {ethBalance}");
        }

        #endregion

        #region ERC20

        public async void ERC20_GetMetadata()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc20ContractAddress, _chainId);
            var metadata = await contract.GetMetadata();
            Debugger.Instance.Log("ERC20 Metadata", JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }

        public async void ERC20_Approve()
        {
            Debugger.Instance.Log("ERC20 Approve", "Approving 0 tokens from ERC20 contract...");
            var wallet = _connectedWallet ?? await GetSmartWallet();
            var contract = await ThirdwebContract.Create(_client, _tokenErc20ContractAddress, _chainId);
            var result = await contract.ERC20_Approve(wallet: wallet, spenderAddress: _tokenErc20ContractAddress, amount: 0);
            Debugger.Instance.Log("ERC20 Approve", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async void ERC20_TotalSupply()
        {
            var contract = await ThirdwebContract.Create(_client, _dropErc20ContractAddress, _chainId);
            var result = await contract.ERC20_TotalSupply();
            Debugger.Instance.Log("ERC20 Total Supply", result.ToString());
        }

        #endregion

        #region ERC721

        public async void ERC721_GetMetadata()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc721ContractAddress, _chainId);
            var metadata = await contract.GetMetadata();
            Debugger.Instance.Log("ERC721 Metadata", JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }

        public async void ERC721_TokenURI()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc721ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");
            var result = await contract.ERC721_TokenURI(tokenId);
            Debugger.Instance.Log("ERC721 Token URI", result);
        }

        public async void ERC721_OwnerOf()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc721ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");
            var result = await contract.ERC721_OwnerOf(tokenId);
            Debugger.Instance.Log("ERC721 OwnerOf", result);
        }

        #endregion

        #region ERC1155

        public async void ERC1155_GetMetadata()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc1155ContractAddress, _chainId);
            var metadata = await contract.GetMetadata();
            Debugger.Instance.Log("ERC1155 Metadata", JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }

        public async void ERC1155_BalanceOf()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc1155ContractAddress, _chainId);
            var wallet = _connectedWallet ?? await GetSmartWallet();
            var address = await wallet.GetAddress();
            var tokenId = 1;
            var result = await contract.ERC1155_BalanceOf(address, tokenId);
            Debugger.Instance.Log("ERC1155 BalanceOf", result.ToString());
        }

        public async void ERC1155_TotalSupply()
        {
            var contract = await ThirdwebContract.Create(_client, _tokenErc1155ContractAddress, _chainId);
            var tokenId = 1;
            var result = await contract.ERC1155_TotalSupply(tokenId);
            Debugger.Instance.Log("ERC1155 TotalSupply", result.ToString());
        }

        #endregion

        #region Drop

        public async void DropERC20_Claim()
        {
            Debugger.Instance.Log("Drop ERC20 Claim", "Claiming 1.5 tokens from DropERC20 contract...");
            var wallet = _connectedWallet ?? await GetSmartWallet();
            var contract = await ThirdwebContract.Create(_client, _dropErc20ContractAddress, _chainId);
            var receiver = await wallet.GetAddress();
            var amount = "1.5";
            var result = await contract.DropERC20_Claim(wallet, receiver, amount);
            Debugger.Instance.Log("Drop ERC20 Claim", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async void DropERC721_GetActiveClaimCondition()
        {
            var contract = await ThirdwebContract.Create(_client, _dropErc721ContractAddress, _chainId);
            var result = await contract.DropERC721_GetActiveClaimCondition();
            Debugger.Instance.Log("Drop ERC721 Active Claim Condition", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async void DropERC1155_Claim()
        {
            Debugger.Instance.Log("Drop ERC1155 Claim", "Claiming 1 token from DropERC1155 contract...");
            var wallet = _connectedWallet ?? await GetSmartWallet();
            var contract = await ThirdwebContract.Create(_client, _dropErc1155ContractAddress, _chainId);
            var receiver = await wallet.GetAddress();
            var tokenId = 1;
            var quantity = 1;
            var result = await contract.DropERC1155_Claim(wallet, receiver, tokenId, quantity);
            Debugger.Instance.Log("Drop ERC1155 Claim", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        #endregion

        #region Token

        public async void TokenERC20_GenerateMintSignature()
        {
            // Generate in backend, take result and call TokenERC20_MintWithSignature client side
            var contract = await ThirdwebContract.Create(_client, _tokenErc20ContractAddress, _chainId);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var randomReceiverAddress = await randomReceiver.GetAddress();
            var amount = 1; // 1 wei
            var mintRequest = new TokenERC20_MintRequest { To = randomReceiverAddress, Quantity = amount, };

            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client); // Should be someone with minter role to make this valid
            (var payload, var signature) = await contract.TokenERC20_GenerateMintSignature(fakeAuthorizedSigner, mintRequest);
            Debugger.Instance.Log("Token ERC20 Generate Mint Signature", $"Signature: {signature}\nPayload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

            // var mintResult = await contract.TokenERC20_MintWithSignature(randomReceiver, payload, signature);
        }

        public async void TokenERC721_GenerateMintSignature()
        {
            // Generate in backend, take result and call TokenERC721_MintWithSignature client side
            var contract = await ThirdwebContract.Create(_client, _tokenErc721ContractAddress, _chainId);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var randomReceiverAddress = await randomReceiver.GetAddress();
            // Empty URI string means no metadata, use ipfs or don't set uri and use NFTMetadata override instead
            var mintRequest = new TokenERC721_MintRequest { To = randomReceiverAddress, Uri = "", };

            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client); // Should be someone with minter role to make this valid

            // set metadataOverride to a valid NFTMetadata object if you want us to upload the metadata and set the URI on your behalf
            (var payload, var signature) = await contract.TokenERC721_GenerateMintSignature(fakeAuthorizedSigner, mintRequest, metadataOverride: null);
            Debugger.Instance.Log("Token ERC721 Generate Mint Signature", $"Signature: {signature}\nPayload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

            // var mintResult = await contract.TokenERC721_MintWithSignature(randomReceiver, payload, signature);
        }

        public async void TokenERC1155_GenerateMintSignature()
        {
            // Generate in backend, take result and call TokenERC1155_MintWithSignature client side
            var contract = await ThirdwebContract.Create(_client, _tokenErc1155ContractAddress, _chainId);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var randomReceiverAddress = await randomReceiver.GetAddress();
            var mintRequest = new TokenERC1155_MintRequest
            {
                To = randomReceiverAddress,
                TokenId = 1,
                Quantity = 1,
            };

            // Since we didn't set a URI let's let thirdweb upload the metadata and set the URI for us
            var metadata = new NFTMetadata
            {
                Name = "Test Token",
                Description = "This is a test token",
                Image = "",
                // More fields can be added here
            };

            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client); // Should be someone with minter role to make this valid
            (var payload, var signature) = await contract.TokenERC1155_GenerateMintSignature(fakeAuthorizedSigner, mintRequest, metadataOverride: metadata);
            Debugger.Instance.Log("Token ERC1155 Generate Mint Signature", $"Signature: {signature}\nPayload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

            // var mintResult = await contract.TokenERC1155_MintWithSignature(randomReceiver, payload, signature);
        }

        #endregion

        #region Signing

        public async void EthSign()
        {
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var message = "Hello World!";
            var result = await wallet.EthSign(message);
            Debugger.Instance.Log("Eth Sign", result);
        }

        public async void PersonalSign()
        {
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var message = "Hello World!";
            var result = await wallet.PersonalSign(message);
            Debugger.Instance.Log("Personal Sign", result);
        }

        public async void SignTypedDataV4()
        {
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var messageJson =
                "{\"types\":{\"EIP712Domain\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"chainId\",\"type\":\"uint256\"},{\"name\":\"verifyingContract\",\"type\":\"address\"}],\"Person\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"wallet\",\"type\":\"address\"}],\"Mail\":[{\"name\":\"from\",\"type\":\"Person\"},{\"name\":\"to\",\"type\":\"Person\"},{\"name\":\"contents\",\"type\":\"string\"}]},\"primaryType\":\"Mail\",\"domain\":{\"name\":\"Ether Mail\",\"version\":\"1\",\"chainId\":421614,\"verifyingContract\":\"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\":{\"from\":{\"name\":\"Cow\",\"wallet\":\"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\":{\"name\":\"Bob\",\"wallet\":\"0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB\"},\"contents\":\"Hello, Bob!\"}}";
            var result = await wallet.SignTypedDataV4(messageJson);
            Debugger.Instance.Log("Sign Typed Data V4", result);
        }

        #endregion

        #region Storage

        public async void Storage_DownloadIPFS()
        {
            var result = await ThirdwebStorage.Download<string>(_client, "ipfs://QmRHf3sBEAaSkaPdjrnYZS7VH1jVgvNBJNoUXmiUyvUpNM/8");
            Debugger.Instance.Log("Storage Download IPFS", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async void Storage_DownloadHTTPS()
        {
            var result = await ThirdwebStorage.Download<string>(_client, "https://raw.githubusercontent.com/thirdweb-dev/thirdweb-dotnet/main/README.md");
            Debugger.Instance.Log("Storage Download HTTP", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async void Storage_UploadScreenshot()
        {
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            var path = Application.persistentDataPath + "/screenshot.png";
            System.IO.File.WriteAllBytes(path, screenshot.EncodeToPNG());
            var result = await ThirdwebStorage.Upload(_client, path);
            Debugger.Instance.Log("Storage Upload Screenshot", JsonConvert.SerializeObject(result, Formatting.Indented));
            Application.OpenURL(result.PreviewUrl);
        }

        #endregion

        #region Custom

        public async void Custom_ContractRead()
        {
            var contractAddress = "0xBC4CA0EdA7647A8aB7C2061c2E118A18a936f13D"; // BAYC
            var chainId = BigInteger.One;
            var contract = await ThirdwebContract.Create(_client, contractAddress, chainId);
            var result = await ThirdwebContract.Read<string>(contract, "name");
            Debugger.Instance.Log("Custom Contract Read", result.ToString());
        }

        public async void Custom_ContractPrepare()
        {
            Debugger.Instance.Log("Custom Contract Prepare", "Preparing an approve transaction for 0 tokens and sending without waiting...");
            var contract = await ThirdwebContract.Create(_client, _tokenErc20ContractAddress, _chainId);
            var wallet = _connectedWallet ?? await GetSmartWallet();
            var tx = await ThirdwebContract.Prepare(wallet: wallet, contract: contract, method: "approve", weiValue: 0, parameters: new object[] { _tokenErc20ContractAddress, 0 });
            var hash = await ThirdwebTransaction.Send(tx);
            Debugger.Instance.Log("Custom Contract Prepare", "Transaction Hash:\n" + hash);
        }

        public async void Custom_ZkSyncAA()
        {
            var zkSyncSepolia = 300;
            var wallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var zkSmartWallet = await SmartWallet.Create(personalWallet: wallet, chainId: zkSyncSepolia, gasless: true);
            // 0 value self transfer essentially, can be any tx
            var txInput = new ThirdwebTransactionInput() { To = await zkSmartWallet.GetAddress(), };
            var tx = await ThirdwebTransaction.Create(wallet: zkSmartWallet, txInput: txInput, chainId: zkSyncSepolia);
            var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(tx);
            Debugger.Instance.Log("Custom ZkSync AA", "Transaction Receipt:\n" + JsonConvert.SerializeObject(receipt, Formatting.Indented));
        }

        #endregion

        #region Smart Wallet

        public async void SmartWallet_AddAdmin()
        {
            Debugger.Instance.Log("Smart Wallet Add Admin", "Adding a random address as admin to Smart Wallet...");
            // Do not use connected wallet for this example, not to pollute default factory
            var personalWallet = await PrivateKeyWallet.Generate(_client);
            var smartWallet = await SmartWallet.Create(personalWallet: personalWallet, chainId: 421614);
            var randomAddress = await (await PrivateKeyWallet.Generate(_client)).GetAddress();
            var receipt = await smartWallet.AddAdmin(randomAddress);
            Debugger.Instance.Log("Smart Wallet Add Admin", JsonConvert.SerializeObject(receipt, Formatting.Indented));
        }

        public async void SmartWallet_RemoveAdmin()
        {
            Debugger.Instance.Log("Smart Wallet Remove Admin", "Removing a random address as admin from Smart Wallet...");
            // Do not use connected wallet for this example, not to pollute default factory
            var personalWallet = await PrivateKeyWallet.Generate(_client);
            var smartWallet = await SmartWallet.Create(personalWallet: personalWallet, chainId: 421614);
            var randomAddress = await (await PrivateKeyWallet.Generate(_client)).GetAddress();
            var receipt = await smartWallet.RemoveAdmin(randomAddress);
            Debugger.Instance.Log("Smart Wallet Remove Admin", JsonConvert.SerializeObject(receipt, Formatting.Indented));
        }

        public async void SmartWallet_CreateSessionKey()
        {
            Debugger.Instance.Log("Smart Wallet Create Session Key", "Creating a session key for a random address...");
            // Do not use connected wallet for this example, not to pollute default factory
            var personalWallet = _connectedWallet ?? await PrivateKeyWallet.Generate(_client);
            var smartWallet = await SmartWallet.Create(personalWallet: personalWallet, chainId: 421614);
            var randomAddress = await (await PrivateKeyWallet.Generate(_client)).GetAddress();
            var receipt = await smartWallet.CreateSessionKey(
                signerAddress: randomAddress,
                approvedTargets: new List<string> { Constants.ADDRESS_ZERO },
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
            );
            Debugger.Instance.Log("Smart Wallet Create Session Key", JsonConvert.SerializeObject(receipt, Formatting.Indented));
        }

        #endregion
    }
}
