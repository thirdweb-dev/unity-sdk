using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

// Your Event data structure
[System.Serializable]
public struct TransferEvent
{
    public string from;
    public string to;
    public string tokenID;

    public override string ToString()
    {
        return
            $"TransferEvent:"
            + $"\n>prevURI: {from}"
            + $"\n>newURI: {to}"
            + $"\n>tokenID: {tokenID}";
    }
}


public class Prefab_Events : MonoBehaviour
{

    // Get all events filtered by name (and optionally add more filters)

    public void GetEventsTest()
    {
        Contract myContract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

        // Optional event query options
        EventQueryOptions myOptions = new EventQueryOptions();
        myOptions.filters = new Dictionary<string, object> { { "tokenID", "1" } };

        GetEvents(myContract, "Transfer", myOptions);
    }

    public async void GetEvents(Contract contract, string eventName, EventQueryOptions eventQueryOptions = null)
    {
        List<ContractEvent<TransferEvent>> allEvents = await contract.events.Get<TransferEvent>(eventName, eventQueryOptions);

        foreach (ContractEvent<TransferEvent> contractEvent in allEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

    // Get all contract events

    public void GetAllEventsTest()
    {
        Contract myContract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

        // Optional event query options
        EventQueryOptions myOptions = new EventQueryOptions();
        myOptions.fromBlock = "100";

        GetAllEvents(myContract, myOptions);
    }

    public async void GetAllEvents(Contract contract, EventQueryOptions eventQueryOptions = null)
    {
        List<ContractEvent<object>> allContractEvents = await contract.events.GetAll(eventQueryOptions);

        foreach (ContractEvent<object> contractEvent in allContractEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

}

