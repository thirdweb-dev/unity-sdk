using System;
using UnityEngine;

namespace link.magic.unity.sdk.Provider
{
    [Serializable]
    public class RelayerResponse<T>
    {
        [SerializeField] internal string msgType;
        [SerializeField] internal MagicRpcResponse<T> response;

        internal RelayerResponse(string msgType, MagicRpcResponse<T> response)
        {
            this.msgType = msgType;
            this.response = response;
        }
    }
}