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
        /// Requests all events from a contract
        /// </summary>
        /// <returns>ContractEvent List</returns>
        public async Task<List<ContractEvent>> GetAllEvents()
        {
            return await Bridge.InvokeRoute<List<ContractEvent>>(getRoute("getAllEvents"), new string[] { });
        }
    }
}