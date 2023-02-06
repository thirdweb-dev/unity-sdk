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

        public async Task<List<ContractEvent<T>>> Get<T>(string eventName, EventQueryOptions eventQueryOptions = null)
        {
            return await Bridge.InvokeRoute<List<ContractEvent<T>>>(getRoute("getEvents"), Utils.ToJsonStringArray(eventName, eventQueryOptions));
        }

        public async Task<List<ContractEvent<object>>> GetAll(EventQueryOptions eventQueryOptions = null)
        {
            return await Bridge.InvokeRoute<List<ContractEvent<object>>>(getRoute("getAllEvents"), Utils.ToJsonStringArray(eventQueryOptions));
        }
    }
}