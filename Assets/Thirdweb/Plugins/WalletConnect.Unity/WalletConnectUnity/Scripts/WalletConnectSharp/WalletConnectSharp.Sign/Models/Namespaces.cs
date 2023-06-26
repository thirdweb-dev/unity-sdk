using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A dictionary of Namespaces based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class Namespaces : Dictionary<string, Namespace>, IEquatable<Namespaces>
    {
        public bool Equals(Namespaces other)
        {
            return new DictionaryComparer<string, Namespace>(Namespace.NamespaceComparer).Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Namespaces)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
