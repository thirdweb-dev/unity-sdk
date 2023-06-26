using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models {

public struct PendingRequestStruct : IKeyHolder<long>
{
    public long Id { get; set; }
    
    public string Topic { get; set; }
    
    public long Key
    {
        get
        {
            return Id;
        }
    }
    
    // Specify object here, so we can store any type
    // We don't care about type-safety for these pending
    // requests
    public SessionRequest<object> Parameters { get; set; }
}
}
