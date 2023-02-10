using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Newtonsoft.Json;
using System;

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

    public async void AddEventListener()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            await contract.events.AddListener("Transfer", (string transferEventStr) => OnTransfer(transferEventStr));

            Debug.Log("Event listener added!");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    public async void RemoveEventListener()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            await contract.events.RemoveListener("Transfer", (string transferEventStr) => OnTransfer(transferEventStr));

            Debug.Log("Event listener removed!");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
    }

    public async void ListenToAllEvents()
    {
        try
        {
            Contract contract = new Contract(
                "goerli",
                "0x2e01763fA0e15e07294D74B63cE4b526B321E389"
            );

            await contract.events.ListenToAll((string contractEventStr) => OnAnyEvent(contractEventStr));

            Debug.Log("Listening to all events!");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error: {e.Message}");
        }
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

    public void OnTransfer(string transferEventStr)
    {
        ContractEvent<TransferEvent> transferEvent = JsonConvert.DeserializeObject<ContractEvent<TransferEvent>>(transferEventStr);
        Debug.Log($"[EventListener] Transfer event was just emitted!\n{transferEvent.ToString()}");
    }

    public void OnAnyEvent(string contractEventStr)
    {
        ContractEvent<object> contractEvent = JsonConvert.DeserializeObject<ContractEvent<object>>(contractEventStr);
        Debug.Log($"[EventListener] An event was just emitted!\n{contractEvent.ToString()}");
    }
}
