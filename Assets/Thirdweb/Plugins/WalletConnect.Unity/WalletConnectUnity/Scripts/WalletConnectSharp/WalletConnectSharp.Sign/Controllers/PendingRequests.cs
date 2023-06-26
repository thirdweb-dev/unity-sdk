using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Controllers {

public class PendingRequests : Store<long, PendingRequestStruct>, IPendingRequests
{
    public PendingRequests(ICore core) : base(core, $"request", WalletConnectSignClient.StoragePrefix)
    {
    }
}
}