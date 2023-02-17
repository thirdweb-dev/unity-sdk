using System.Collections;
using UnityEngine;
using Nethereum.Unity.Rpc;
using System.Numerics;

public class Blocks : MonoBehaviour
{
    void Start()
    {
        GetBlockNumber();
    }

    public void GetBlockNumber()
    {
        StartCoroutine(GetBlockNumber_Routine());
    }

    IEnumerator GetBlockNumber_Routine()
    {
        EthBlockNumberUnityRequest blockNumberRequest = new EthBlockNumberUnityRequest("https://eth-mainnet.g.alchemy.com/v2/EhZoPKWlYZKh6-kkNkI9bT4pEbSccp_2");
        yield return blockNumberRequest.SendRequest();
        if (blockNumberRequest.Exception == null)
        {
            BigInteger blockNumber = blockNumberRequest.Result.Value;
            Debug.Log($"Block: {blockNumber.ToString()}");
        }
    }
}
