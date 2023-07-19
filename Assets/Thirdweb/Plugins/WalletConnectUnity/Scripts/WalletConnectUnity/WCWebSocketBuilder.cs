using UnityEngine;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Interfaces;

namespace WalletConnect
{
    public class WCWebSocketBuilder : MonoBehaviour, IConnectionBuilder
    {
        public IJsonRpcConnection CreateConnection(string url)
        {
            var websocket = gameObject.AddComponent<WCWebSocket>();
            websocket.Url = url;

            return websocket;
        }
    }
}