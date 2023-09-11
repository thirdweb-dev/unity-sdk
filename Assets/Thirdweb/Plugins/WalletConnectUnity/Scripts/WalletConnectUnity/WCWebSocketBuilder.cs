using System.Threading.Tasks;
using UnityEngine;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Interfaces;
using WalletConnectUnity.Utils;

namespace WalletConnect
{
    public class WCWebSocketBuilder : MonoBehaviour, IConnectionBuilder
    {
        public Task<IJsonRpcConnection> CreateConnection(string url)
        {
            TaskCompletionSource<IJsonRpcConnection> taskCompletionSource =
                new TaskCompletionSource<IJsonRpcConnection>();
            
            MTQ.Enqueue(() =>
            {
                Debug.Log("Building websocket with URL " + url);
                var websocket = gameObject.AddComponent<WCWebSocket>();
                websocket.Url = url;
                
                taskCompletionSource.TrySetResult(websocket);
            });

            
            return taskCompletionSource.Task;
        }
    }
}