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
    /// A dictionary of RequiredNamespace based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class RequiredNamespaces : Dictionary<string, RequiredNamespace>, IEquatable<RequiredNamespaces>
    {
        public bool Equals(RequiredNamespaces other)
        {
            return new DictionaryComparer<string, RequiredNamespace>(RequiredNamespace.RequiredNamespaceComparer).Equals(this, other);
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

            return Equals((RequiredNamespaces)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }


        public bool Equals(RequiredNamespaces x, RequiredNamespaces y)
        {
            return new DictionaryComparer<string, RequiredNamespace>(RequiredNamespace.RequiredNamespaceComparer).Equals(x, y);
        }

        public int GetHashCode(RequiredNamespaces obj)
        {
            throw new NotImplementedException();
        }
    }
}
