using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thirdweb
{
    public class Events : Routable
    {
        public Events(string parentRoute) : base(Routable.append(parentRoute, "events"))
        {

        }

        // READ FUNCTIONS

        /// <summary>
        /// Request specific events from a contract
        /// </summary>
        /// <param name="eventName">The event name as defined in the contract</param>
        /// <param name="eventQueryOptions">Optional query filters</param>
        /// <typeparam name="T">The event data structure to deserialize into</typeparam>
        /// <returns>List of ContractEvent<T></returns>
        public async Task<List<ContractEvent<T>>> Get<T>(string eventName, EventQueryOptions eventQueryOptions = null)
        {
            return await Bridge.InvokeRoute<List<ContractEvent<T>>>(getRoute("getEvents"), Utils.ToJsonStringArray(eventName, eventQueryOptions));
        }

        /// <summary>
        /// Request all events from a contract
        /// </summary>
        /// <param name="eventQueryOptions">Optional query filters</param>
        /// <returns>List of ContractEvent<object></returns>
        public async Task<List<ContractEvent<object>>> GetAll(EventQueryOptions eventQueryOptions = null)
        {
            return await Bridge.InvokeRoute<List<ContractEvent<object>>>(getRoute("getAllEvents"), Utils.ToJsonStringArray(eventQueryOptions));
        }
    }
}