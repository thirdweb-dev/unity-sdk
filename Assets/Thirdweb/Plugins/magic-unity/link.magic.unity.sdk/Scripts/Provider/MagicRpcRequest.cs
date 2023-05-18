using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace link.magic.unity.sdk.Provider
{
    [Serializable]
    public class MagicRpcRequest<T>
    {
        [SerializeField] internal int id;
        [SerializeField] internal string jsonrpc = "2.0";
        [SerializeField] internal string method;
        [SerializeField] internal T[] @params;

        public MagicRpcRequest(string method, T[] parameters)
        {
            id = Random.Range(1, 100000);
            this.method = method;
            @params = parameters;
        }
    }
}