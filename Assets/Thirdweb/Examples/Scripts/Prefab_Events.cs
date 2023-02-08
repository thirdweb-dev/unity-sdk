using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

// Your Event data structure
[System.Serializable]
public struct TransferEvent
{
    public string from;
    public string to;
    public string tokenId;

    public override string ToString()
    {
        return
            $"TransferEvent:"
            + $"\n>prevURI: {from}"
            + $"\n>newURI: {to}"
            + $"\n>tokenId: {tokenId}";
    }
}


public class Prefab_Events : MonoBehaviour
{

    // Get all events filtered by name (and optionally add more filters)

    public async void GetEventsTest()
    {
        Contract contract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

        // Optional event query options
        Dictionary<string, object> filters = new Dictionary<string, object> { { "tokenId", 20 } };
        EventQueryOptions options = new EventQueryOptions(filters);

        List<ContractEvent<TransferEvent>> allEvents = await contract.events.Get<TransferEvent>("Transfer", options);

        foreach (ContractEvent<TransferEvent> contractEvent in allEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

    // Get all contract events

    public async void GetAllEventsTest()
    {
        Contract contract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

        // Optional event query options
        EventQueryOptions options = new EventQueryOptions(null, 0, 16500000, "desc");

        List<ContractEvent<object>> allContractEvents = await contract.events.GetAll(options);

        foreach (ContractEvent<object> contractEvent in allContractEvents)
            Debug.Log($"{contractEvent.ToString()}\n");
    }

}

