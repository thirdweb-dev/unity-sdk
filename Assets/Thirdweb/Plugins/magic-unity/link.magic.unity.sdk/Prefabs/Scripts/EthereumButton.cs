using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using link.magic.unity.sdk;
using link.magic.unity.sdk.Provider;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.RPC.Personal;
using UnityEngine;
using UnityEngine.UI;

public class EthereumButton : MonoBehaviour
{
    private string _account;
    
    public Text result;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Update is called once per frame
    public async void SendTransaction()
    {
        result.text = "";
        var ethAccounts = new EthAccounts(Magic.Instance.Provider);
        var accounts = await ethAccounts.SendRequestAsync();
        var transaction = new EthSendTransaction(Magic.Instance.Provider);
        var transactionInput = new TransactionInput
            { To = accounts[0], Value = new HexBigInteger(10), From = accounts[0]};
        var hash = await transaction.SendRequestAsync(transactionInput);
        Debug.Log(hash);
        result.text = hash;
    }
    
    public async void GetAccount()
    {
        result.text = "";
        var ethAccounts = new EthAccounts(Magic.Instance.Provider);
        var accounts = await ethAccounts.SendRequestAsync();
        _account = accounts[0];
        Debug.Log(accounts[0]);
        result.text = accounts[0];
    }

    public async void EthSign()
    {
        result.text = "";
        
        var ethAccounts = new EthAccounts(Magic.Instance.Provider);
        var accounts = await ethAccounts.SendRequestAsync();
        var personalSign = new EthSign(Magic.Instance.Provider);
        var transactionInput = new TransactionInput{Data = "Hello world"};
        var res = await personalSign.SendRequestAsync(accounts[0], "hello world");
        result.text = res;
    }

    public async void GetBalance()
    {
        var ethApiService = new EthApiService(Magic.Instance.Provider);
        var accounts = await ethApiService.Accounts.SendRequestAsync();
        var balance = await ethApiService.GetBalance.SendRequestAsync(accounts[0]);
        result.text = balance.ToString();
    }
    
    public async void GetChainId()
    {
        var ethApiService = new EthApiService(Magic.Instance.Provider);
        var chainId = await ethApiService.ChainId.SendRequestAsync();
        result.text = chainId.ToString();
    }
}
