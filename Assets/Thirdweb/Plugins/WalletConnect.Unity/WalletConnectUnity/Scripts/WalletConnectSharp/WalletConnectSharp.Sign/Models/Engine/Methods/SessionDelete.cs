using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionDelete. Used to delete
    /// a session 
    /// </summary>
    [RpcMethod("wc_sessionDelete")]
    [RpcRequestOptions(Clock.ONE_DAY, 1112)]
    [RpcResponseOptions(Clock.ONE_DAY, 1113)]
    public class SessionDelete : Error, IWcMethod
    {
    }
}
