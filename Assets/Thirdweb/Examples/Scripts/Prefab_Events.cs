using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

// Your Event data structure
[System.Serializable]
public struct MyEvent
{
    public string prevURI;
    public string newURI;

    public override string ToString()
    {
        return
            $"MyEvent:"
            + $"\n>prevURI: {prevURI}"
            + $"\n>newURI: {newURI.ToString()}";
    }
}


public class Prefab_Events : MonoBehaviour
{

    // Get all events filtered by name (and optionally add more filters)

    public void GetEventsTest()
    {
        Contract myContract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");
        GetEvents(myContract, "ContractURIUpdated");
    }

    public async void GetEvents(Contract contract, string eventName, EventQueryOptions eventQueryOptions = null)
    {
        List<ContractEvent<MyEvent>> allEvents = await contract.events.Get<MyEvent>(eventName, eventQueryOptions);

        foreach (ContractEvent<MyEvent> contractEvent in allEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

    // Get all contract events

    public void GetAllEventsTest()
    {
        Contract myContract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

        // Optional event query options
        EventQueryOptions myOptions = new EventQueryOptions();
        Dictionary<string, object> myFilters = new Dictionary<string, object> {
            { "newURI", "ipfs://QmNckvfgMj6WUeGszGToiptXYJx2DLPQpZ5RUhoMcDa9JD/0" }
        };
        myOptions.filters = myFilters;
        myOptions.fromBlock = "0";

        GetAllEvents(myContract, myOptions);
    }

    public async void GetAllEvents(Contract contract, EventQueryOptions eventQueryOptions = null)
    {
        List<ContractEvent<object>> allContractEvents = await contract.events.GetAll(eventQueryOptions);

        foreach (ContractEvent<object> contractEvent in allContractEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

}

