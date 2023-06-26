using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Models.Pairing.Methods
{
    /// <summary>
    /// A class that represents the request wc_pairingPing. Used to ping a pairing
    /// request
    /// </summary>
    [RpcMethod("wc_pairingPing")]
    [RpcRequestOptions(Clock.THIRTY_SECONDS, 1002)]
    [RpcResponseOptions(Clock.THIRTY_SECONDS, 1003)]
    public class PairingPing : Dictionary<string, object>
    {
    }
}
