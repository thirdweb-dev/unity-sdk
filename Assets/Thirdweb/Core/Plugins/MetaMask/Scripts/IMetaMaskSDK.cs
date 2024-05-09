using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaMask.Models;
using MetaMask.Transports.Unity;
using MetaMask.Unity.Models;

namespace MetaMask.Unity
{
    public interface IMetaMaskSDK : IMetaMaskEvents
    {
        string InfuraProjectId { get; }

        IAppConfig Config { get; }
        
        string SDKVersion { get; }

        List<MetaMaskUnityRpcUrlConfig> RpcUrl { get; }
        
        event EventHandler MetaMaskUnityBeforeInitialized;
        
        event EventHandler MetaMaskUnityInitialized;
        
        event EventHandler<MetaMaskUnityRequestEventArgs> Requesting;
        
        MetaMaskWallet Wallet { get; }

        void Initialize();

        void Connect();

        Task<string> ConnectAndSign(string message);

        Task<TR> ConnectWith<TR>(string method, params object[] @params);

        Task ConnectAndBatch(BatchRequester requests);

        void Disconnect(bool endSession = false);

        void EndSession();

        bool IsInUnityThread();

        Task<object> Request(MetaMaskEthereumRequest request);

        object Request(string method, object[] parameters = null);

        Task<TR> Request<TR>(string method, object[] parameters = null);

        BatchRequester BatchRequests();

        void SaveSession();

        bool IsWebGL();
    }
}