using System.Collections.Generic;
using UnityEngine;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Thirdweb.Examples
{
    // Your Event type (WebGL)
    [System.Serializable]
    public struct TransferEvent
    {
        public string from;
        public string to;
        public string tokenId;

        public override string ToString()
        {
            return $"TransferEvent:" + $"\n>from: {from}" + $"\n>to: {to}" + $"\n>tokenId: {tokenId}";
        }
    }

    // Your Event type (Native platforms)
    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public BigInteger TokenId { get; set; }

        public override string ToString()
        {
            return $"TransferEvent:" + $"\n>from: {From}" + $"\n>to: {To}" + $"\n>tokenId: {TokenId}";
        }
    }

    public class Prefab_Events : MonoBehaviour
    {
        // Get all events filtered by name (and optionally add more filters)

        public async void GetEvents()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x345E7B4CCA26725197f1Bed802A05691D8EF7770");

                if (Utils.IsWebGLBuild())
                {
                    // Optional event query options
                    Dictionary<string, object> filters = new Dictionary<string, object> { { "tokenId", 0 } };
                    EventQueryOptions options = new EventQueryOptions(filters);

                    List<ContractEvent<TransferEvent>> allEvents = await contract.Events.Get<TransferEvent>("Transfer", options);
                    Debugger.Instance.Log("[Get Events] Get - TransferEvent #1", allEvents[0].ToString());
                }
                else
                {
                    // Optional event query options
                    ulong? fromBlock = null;
                    ulong? toBlock = null;

                    var allEvents = await contract.GetEventLogs<TransferEventDTO>(fromBlock, toBlock);
                    Debugger.Instance.Log("[Get Events] Get - TransferEvent #1", allEvents[0].Event.ToString());
                }
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Get Events] Error", e.Message);
            }
        }

        // Get all contract events

        public async void GetAllEvents()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x345E7B4CCA26725197f1Bed802A05691D8EF7770");

                // Optional event query options
                EventQueryOptions options = new EventQueryOptions(null, 0, 16500000, "desc");

                List<ContractEvent<object>> allContractEvents = await contract.Events.GetAll(options);
                Debugger.Instance.Log("[Get All Events] Get - ContractEvent #1", allContractEvents[0].ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Get All Events] Error", e.Message);
            }
        }

        // Event listeners

        public void ListenToAllEvents()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x345E7B4CCA26725197f1Bed802A05691D8EF7770");
                contract.Events.ListenToAll((ContractEvent<object> anyEvent) => OnEventTriggered(anyEvent));
                Debugger.Instance.Log("Listening to all events!", "Try to trigger an event on the specified contract to get a callback.");
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Listen To All Events] Error", e.Message);
            }
        }

        public async void RemoveAllEventListeners()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x345E7B4CCA26725197f1Bed802A05691D8EF7770");
                await contract.Events.RemoveAllListeners();
                Debugger.Instance.Log("Removed all event listeners!", "Events emitted will not trigger callbacks anymore.");
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Remove All Event Listeners] Error", e.Message);
            }
        }

        public void OnEventTriggered<T>(ContractEvent<T> contractEvent)
        {
            Debugger.Instance.Log("[EventListener] OnEventTriggered", $"An event was just emitted!\n{contractEvent.ToString()}");
        }
    }
}
