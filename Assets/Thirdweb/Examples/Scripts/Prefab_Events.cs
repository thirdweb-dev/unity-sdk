using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

public class Prefab_Events : MonoBehaviour
{
    private void Start()
    {
        Contract contract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");
        GetAllEvents(contract);
    }

    public async void GetAllEvents(Contract contract)
    {
        List<ContractEvent> allContractEvents = await contract.GetAllEvents();
        foreach (ContractEvent contractEvent in allContractEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }
}
