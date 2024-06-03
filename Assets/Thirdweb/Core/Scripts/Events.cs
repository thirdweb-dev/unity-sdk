using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

namespace Thirdweb
{
    public class Events : Routable
    {
        private readonly ThirdwebSDK _sdk;

        public Events(ThirdwebSDK sdk, string parentRoute)
            : base(Routable.append(parentRoute, "events"))
        {
            _sdk = sdk;
        }

        // READ FUNCTIONS

        /// <summary>
        /// Request specific events from a contract
        /// /// </summary>
        /// <param name="eventName">The event name as defined in the contract</param>
        /// <param name="eventQueryOptions">Optional query filters</param>
        /// <typeparam name="T">The event data structure to deserialize into</typeparam>
        /// <returns>List of ContractEvent<T></returns>
        public async Task<List<ContractEvent<T>>> Get<T>(string eventName, EventQueryOptions eventQueryOptions = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<ContractEvent<T>>>(getRoute("getEvents"), Utils.ToJsonStringArray(eventName, eventQueryOptions));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Request all events from a contract
        /// </summary>
        /// <param name="eventQueryOptions">Optional query filters</param>
        /// <returns>List of ContractEvent<object></returns>
        public async Task<List<ContractEvent<object>>> GetAll(EventQueryOptions eventQueryOptions = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<ContractEvent<object>>>(getRoute("getAllEvents"), Utils.ToJsonStringArray(eventQueryOptions));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Listens to all events and executes callback every time
        /// </summary>
        /// <param name="action">Callback action</param>
        /// <typeparam name="T">Action return type</typeparam>
        /// <returns>Task ID string</returns>
        public string ListenToAll<T>(Action<T> action)
        {
            if (Utils.IsWebGLBuild())
            {
                return Bridge.InvokeListener(getRoute("listenToAllEvents"), new string[] { }, action);
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Removes all event listeners
        /// </summary>
        public async Task<string> RemoveAllListeners()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("removeAllListeners"), new string[] { });
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }
}
