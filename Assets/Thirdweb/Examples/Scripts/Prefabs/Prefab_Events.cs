using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Newtonsoft.Json;

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

    public async void GetEvents()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            // Optional event query options
            Dictionary<string, object> filters = new Dictionary<string, object>
            {
                { "tokenId", 20 }
            };
            EventQueryOptions options = new EventQueryOptions(filters);

            List<ContractEvent<TransferEvent>> allEvents = await contract.events.Get<TransferEvent>(
                "Transfer",
                options
            );

            foreach (ContractEvent<TransferEvent> contractEvent in allEvents)
                Debug.Log($"{contractEvent.ToString()}\n");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    // Get all contract events

    public async void GetAllEvents()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            // Optional event query options
            EventQueryOptions options = new EventQueryOptions(null, 0, 16500000, "desc");

            List<ContractEvent<object>> allContractEvents = await contract.events.GetAll(options);

            foreach (ContractEvent<object> contractEvent in allContractEvents)
                Debug.Log($"{contractEvent.ToString()}\n");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    // Event listeners

    public void AddEventListener()
    {
        Contract contract = new Contract(
            "goerli",
            "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
        );

        contract.events.AddListener("Transfer", (ContractEvent<TransferEvent> transferEvent) => OnTransferEventTriggered(transferEvent));

        Debug.Log("Event listener added!");
    }

    public void ListenToAllEvents()
    {
        Contract contract = new Contract(
            "goerli",
            "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
        );

        contract.events.ListenToAll((ContractEvent<object> anyEvent) => OnEventTriggered(anyEvent));

        Debug.Log("Listening to all events!");
    }

    public async void RemoveAllEventListeners()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            await contract.events.RemoveAllListeners();

            Debug.Log("Removed all event listeners!");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    public void OnTransferEventTriggered(ContractEvent<TransferEvent> transferEvent)
    {
        Debug.Log($"[EventListener] A transfer event was just emitted!\n{transferEvent.ToString()}");
    }

    public void OnEventTriggered<T>(ContractEvent<T> contractEvent)
    {
        Debug.Log($"[EventListener] An event was just emitted!\n{contractEvent.ToString()}");
    }
}
