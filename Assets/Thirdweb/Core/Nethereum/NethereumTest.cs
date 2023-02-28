using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Thirdweb;
using UnityEngine;

using Nethereum.Unity.Rpc;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.Model;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Util;

using ERC20Query = Nethereum.Unity.Contracts.Standards.ERC20;
using ERC721Query = Nethereum.Unity.Contracts.Standards.ERC721;
using ERC1155Query = Nethereum.Unity.Contracts.Standards.ERC1155;

using ERC20Contract = Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using ERC721Contract = Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using ERC1155Contract = Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Web3;

public class NethereumTest : MonoBehaviour
{
    string rpc = "https://goerli.blockpi.network/v1/rpc/public";
    string account = "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34";
    string testERC20ContractAddress = "0xB4870B21f80223696b68798a755478C86ce349bE";
    string testERC721ContractAddress = "0x2e01763fA0e15e07294D74B63cE4b526B321E389";
    string testERC1155ContractAddress = "0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A";

    Web3 web3;

    private void Awake()
    {
        web3 = new Web3(rpc);
    }

    async void Start()
    {
        // ERC20
        Thirdweb.Contract erc20Contract = ThirdwebManager.Instance.SDK.GetContract(testERC20ContractAddress, "[{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_thirdwebFee\",\"type\":\"address\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"delegator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"fromDelegate\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"toDelegate\",\"type\":\"address\"}],\"name\":\"DelegateChanged\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"delegate\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"previousBalance\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newBalance\",\"type\":\"uint256\"}],\"name\":\"DelegateVotesChanged\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Paused\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"platformFeeRecipient\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"platformFeeBps\",\"type\":\"uint256\"}],\"name\":\"PlatformFeeInfoUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"}],\"name\":\"PrimarySaleRecipientUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"previousAdminRole\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"newAdminRole\",\"type\":\"bytes32\"}],\"name\":\"RoleAdminChanged\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"}],\"name\":\"RoleGranted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"}],\"name\":\"RoleRevoked\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"mintedTo\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"quantityMinted\",\"type\":\"uint256\"}],\"name\":\"TokensMinted\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"signer\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"mintedTo\",\"type\":\"address\"},{\"components\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"primarySaleRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"quantity\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"price\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"currency\",\"type\":\"address\"},{\"internalType\":\"uint128\",\"name\":\"validityStartTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"uint128\",\"name\":\"validityEndTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"bytes32\",\"name\":\"uid\",\"type\":\"bytes32\"}],\"indexed\":false,\"internalType\":\"struct ITokenERC20.MintRequest\",\"name\":\"mintRequest\",\"type\":\"tuple\"}],\"name\":\"TokensMintedWithSignature\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"Unpaused\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"DEFAULT_ADMIN_ROLE\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"DOMAIN_SEPARATOR\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"burn\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"burnFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint32\",\"name\":\"pos\",\"type\":\"uint32\"}],\"name\":\"checkpoints\",\"outputs\":[{\"components\":[{\"internalType\":\"uint32\",\"name\":\"fromBlock\",\"type\":\"uint32\"},{\"internalType\":\"uint224\",\"name\":\"votes\",\"type\":\"uint224\"}],\"internalType\":\"struct ERC20VotesUpgradeable.Checkpoint\",\"name\":\"\",\"type\":\"tuple\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"contractType\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"contractURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"contractVersion\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"subtractedValue\",\"type\":\"uint256\"}],\"name\":\"decreaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"delegatee\",\"type\":\"address\"}],\"name\":\"delegate\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"delegatee\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"expiry\",\"type\":\"uint256\"},{\"internalType\":\"uint8\",\"name\":\"v\",\"type\":\"uint8\"},{\"internalType\":\"bytes32\",\"name\":\"r\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"s\",\"type\":\"bytes32\"}],\"name\":\"delegateBySig\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"delegates\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"blockNumber\",\"type\":\"uint256\"}],\"name\":\"getPastTotalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"blockNumber\",\"type\":\"uint256\"}],\"name\":\"getPastVotes\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getPlatformFeeInfo\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint16\",\"name\":\"\",\"type\":\"uint16\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"}],\"name\":\"getRoleAdmin\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"index\",\"type\":\"uint256\"}],\"name\":\"getRoleMember\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"}],\"name\":\"getRoleMemberCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"getVotes\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"grantRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"hasRole\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"addedValue\",\"type\":\"uint256\"}],\"name\":\"increaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_defaultAdmin\",\"type\":\"address\"},{\"internalType\":\"string\",\"name\":\"_name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_symbol\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"_contractURI\",\"type\":\"string\"},{\"internalType\":\"address[]\",\"name\":\"_trustedForwarders\",\"type\":\"address[]\"},{\"internalType\":\"address\",\"name\":\"_primarySaleRecipient\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_platformFeeRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_platformFeeBps\",\"type\":\"uint256\"}],\"name\":\"initialize\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"forwarder\",\"type\":\"address\"}],\"name\":\"isTrustedForwarder\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"mintTo\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"primarySaleRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"quantity\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"price\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"currency\",\"type\":\"address\"},{\"internalType\":\"uint128\",\"name\":\"validityStartTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"uint128\",\"name\":\"validityEndTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"bytes32\",\"name\":\"uid\",\"type\":\"bytes32\"}],\"internalType\":\"struct ITokenERC20.MintRequest\",\"name\":\"_req\",\"type\":\"tuple\"},{\"internalType\":\"bytes\",\"name\":\"_signature\",\"type\":\"bytes\"}],\"name\":\"mintWithSignature\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes[]\",\"name\":\"data\",\"type\":\"bytes[]\"}],\"name\":\"multicall\",\"outputs\":[{\"internalType\":\"bytes[]\",\"name\":\"results\",\"type\":\"bytes[]\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"}],\"name\":\"nonces\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"numCheckpoints\",\"outputs\":[{\"internalType\":\"uint32\",\"name\":\"\",\"type\":\"uint32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"pause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"paused\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"deadline\",\"type\":\"uint256\"},{\"internalType\":\"uint8\",\"name\":\"v\",\"type\":\"uint8\"},{\"internalType\":\"bytes32\",\"name\":\"r\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"s\",\"type\":\"bytes32\"}],\"name\":\"permit\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"primarySaleRecipient\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"renounceRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"role\",\"type\":\"bytes32\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"revokeRole\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"}],\"name\":\"setContractURI\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_platformFeeRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_platformFeeBps\",\"type\":\"uint256\"}],\"name\":\"setPlatformFeeInfo\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_saleRecipient\",\"type\":\"address\"}],\"name\":\"setPrimarySaleRecipient\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"interfaceId\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unpause\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"primarySaleRecipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"quantity\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"price\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"currency\",\"type\":\"address\"},{\"internalType\":\"uint128\",\"name\":\"validityStartTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"uint128\",\"name\":\"validityEndTimestamp\",\"type\":\"uint128\"},{\"internalType\":\"bytes32\",\"name\":\"uid\",\"type\":\"bytes32\"}],\"internalType\":\"struct ITokenERC20.MintRequest\",\"name\":\"_req\",\"type\":\"tuple\"},{\"internalType\":\"bytes\",\"name\":\"_signature\",\"type\":\"bytes\"}],\"name\":\"verify\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"}]");

        Currency currency = await erc20Contract.ERC20.Get();
        Debug.Log(currency.ToString());

        CurrencyValue balanceOf = await erc20Contract.ERC20.BalanceOf(account);
        Debug.Log(balanceOf.ToString());

        CurrencyValue allowanceOf = await erc20Contract.ERC20.AllowanceOf(account, account);
        Debug.Log(allowanceOf.ToString());

        CurrencyValue totalSupply = await erc20Contract.ERC20.TotalSupply();
        Debug.Log(totalSupply.ToString());


        // ERC721
        Thirdweb.Contract erc721Contract = ThirdwebManager.Instance.SDK.GetContract(testERC721ContractAddress);
        NFT nft = await erc721Contract.ERC721.Get("0");
        Debug.Log(nft.ToString());

        List<NFT> allNfts = await erc721Contract.ERC721.GetAll();

        int balanceOfResult = await erc20Contract.Read<int>("balanceOf", account);
        Debug.Log("Custom Read Call: " + balanceOfResult.ToString());
        // StartCoroutine(TestInOrder());
    }

    IEnumerator TestInOrder()
    {
        // BlockNumber();
        yield return StartCoroutine(GetBlockNumber());

        // NativeBalance();
        yield return StartCoroutine(GetNativeBalance());

        // ERC20Balance();
        yield return StartCoroutine(GetERC20Balance());

        // ERC721Balance();
        yield return StartCoroutine(GetERC721Balance());

        // ERC1155Balance();
        yield return StartCoroutine(GetERC1155Balance());

        // Event();
        yield return StartCoroutine(GetEvent());
    }

    // Get Block

    IEnumerator GetBlockNumber()
    {
        var req = new EthBlockNumberUnityRequest(rpc);
        yield return req.SendRequest();
        BigInteger blockNumber = req.Result.Value;
        Debug.Log($"Block: {blockNumber}");
    }

    // async void BlockNumber()
    // {
    //     // ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
    //     var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
    //     Debug.Log($"Block: {blockNumber}");
    // }

    // Get Native Balance

    IEnumerator GetNativeBalance()
    {

        var req = new EthGetBalanceUnityRequest(rpc);
        yield return req.SendRequest(account, BlockParameter.CreateLatest());
        BigInteger balance = req.Result.Value;
        Debug.Log($"Native Balance: {balance}");
    }

    // async void NativeBalance()
    // {
    //     BigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(account);
    //     Debug.Log($"Native Balance: {balance}");
    // }

    // Get ERC20 Balance

    IEnumerator GetERC20Balance()
    {
        var req = new ERC20Query.BalanceOfQueryRequest(rpc, testERC20ContractAddress);
        yield return req.Query(account);
        BigInteger balance = req.Result.Balance;
        Debug.Log($"ERC20 Balance: {balance}");
    }

    // async void ERC20Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC20.GetContractService(testERC20ContractAddress).BalanceOfQueryAsync(account);
    //     Debug.Log($"ERC20 Balance: {balance}");
    // }

    // Get ERC721 Balance

    IEnumerator GetERC721Balance()
    {
        var req = new ERC721Query.BalanceOfQueryRequest(rpc, testERC721ContractAddress);
        yield return req.Query(account);
        BigInteger balance = req.Result.ReturnValue1;
        Debug.Log($"ER721 Balance: {balance}");
    }

    // async void ERC721Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC721.GetContractService(testERC721ContractAddress).BalanceOfQueryAsync(account);
    //     Debug.Log($"ER721 Balance: {balance}");
    // }

    // Get ERC1155 Balance

    IEnumerator GetERC1155Balance()
    {
        var req = new ERC1155Query.BalanceOfQueryRequest(rpc, testERC1155ContractAddress);
        yield return req.Query(account, BigInteger.Parse("98907841070571154799872550564665626750057551199604130120461408711760764993537"));
        BigInteger balance = req.Result.ReturnValue1;
        Debug.Log($"ERC1155 Balance: {balance}");
    }

    // async void ERC1155Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC1155.GetContractService(testERC1155ContractAddress).BalanceOfQueryAsync(account, BigInteger.Parse("98907841070571154799872550564665626750057551199604130120461408711760764993537"));
    //     Debug.Log($"ERC1155 Balance: {balance}");
    // }

    // Get Events

    IEnumerator GetEvent()
    {
        var req = new EthGetLogsUnityRequest(rpc);
        var eventTransfer = EventExtensions.GetEventABI<ERC20Contract.TransferEventDTO>();
        yield return req.SendRequest(eventTransfer.CreateFilterInput(testERC20ContractAddress, account));
        List<EventLog<ERC20Contract.TransferEventDTO>> events = req.Result.DecodeAllEvents<ERC20Contract.TransferEventDTO>();
        Debug.Log($"Event Decoded #1 | Event: {events[0].Event.ToString()} | Block Number: {events[0].Log.BlockNumber}");
    }

    // async void Event()
    // {
    //     var eventHandler = web3.Eth.GetEvent<ERC20Contract.TransferEventDTO>(testERC20ContractAddress);
    //     var eventFilter = eventHandler.CreateFilterInput(account);
    //     var events = await eventHandler.GetAllChangesAsync(eventFilter);

    //     Debug.Log($"Event Decoded #1 | Event: {events[0].Event.ToString()} | Block Number: {events[0].Log.BlockNumber}");
    // }

    // Transfer Ether

    // IEnumerator TransferEther()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;

    //     // Send TX
    //     var transferReq = new EthTransferUnityRequest(rpc, privateKey, 80001);
    //     yield return transferReq.TransferEther(receivingAddress, 1.1m, 2);
    //     string transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer Ether] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer Ether] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Balance
    //     var balanceReq = new EthGetBalanceUnityRequest(rpc);
    //     yield return balanceReq.SendRequest(receivingAddress, BlockParameter.CreateLatest());
    //     Debug.Log("[Transfer Ether] Balance Of Receiving Address: " + UnitConversion.Convert.FromWei(balanceReq.Result.Value));
    // }

    // // Transfer ERC20

    // IEnumerator TransferERC20()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var amount = 1000;

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC20Contract.TransferFunction
    //     {
    //         To = receivingAddress,
    //         Value = amount,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC20ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC20] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC20] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC20Contract.TransferEventDTO>();
    //     Debug.Log("[Transfer ERC20] Transfer Event: " + transferEvent[0].Event.ToString());
    // }

    // // Transfer ERC721

    // IEnumerator TransferERC721()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var tokenId = 1;

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC721Contract.TransferFromFunction
    //     {
    //         From = account,
    //         To = receivingAddress,
    //         TokenId = tokenId,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC721ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC721] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC721] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC721Contract.TransferEventDTO>();
    //     Debug.Log("[Transfer ERC721] Transfer Event: " + transferEvent[0].Event.ToString());
    // }

    // // Transfer ERC1155

    // IEnumerator TransferERC1155()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var tokenId = 1;
    //     var amount = 1;
    //     var data = new byte[0];

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC1155Contract.SafeTransferFromFunction
    //     {
    //         From = account,
    //         To = receivingAddress,
    //         Id = tokenId,
    //         Amount = amount,
    //         Data = data,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC1155ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC1155] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC1155] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC1155Contract.SafeTransferFromFunction>();
    //     Debug.Log("[Transfer ERC1155] Transfer Event: " + transferEvent[0].Event.ToString());
    // }
}
