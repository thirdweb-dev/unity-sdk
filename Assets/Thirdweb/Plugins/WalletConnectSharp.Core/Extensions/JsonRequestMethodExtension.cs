using System.Linq;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Core.Extensions
{
    public static class JsonRequestMethodExtension
    {
        public static string[] ToStringArray(this JsonRpcRequestMethod[] methods) => methods.Select(m => m.ToString()).ToArray();
    }
}
