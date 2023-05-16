using System;
using UnityEngine;

namespace link.magic.unity.sdk.Provider
{
    [Serializable]
    internal class MagicRpcResponse<T>
    {
        [SerializeField] internal int id;
        [SerializeField] internal string jsonrpc = "2.0";
        [SerializeField] internal T result;
        [SerializeField] internal Error error;

        public MagicRpcResponse(int id, string jsonrpc, T result)
        {
            this.id = id;
            this.jsonrpc = jsonrpc;
            this.result = result;
        }
    }

    [Serializable]
    public class Error
    {
        public int code;
        public string message;
    }
}