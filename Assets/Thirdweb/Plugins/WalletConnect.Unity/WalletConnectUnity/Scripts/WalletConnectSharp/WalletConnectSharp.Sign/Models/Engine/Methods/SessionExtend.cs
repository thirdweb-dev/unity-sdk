using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionExtend. Used to extend a session
    /// </summary>
    [RpcMethod("wc_sessionExtend")]
    [RpcRequestOptions(Clock.ONE_DAY, 1106)]
    [RpcResponseOptions(Clock.ONE_DAY, 1107)]
    public class SessionExtend : Dictionary<string, object>, IWcMethod
    {
    }
}
