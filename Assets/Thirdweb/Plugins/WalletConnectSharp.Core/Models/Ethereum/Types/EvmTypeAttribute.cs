using System;

namespace WalletConnectSharp.Core.Models.Ethereum.Types
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EvmTypeAttribute : Attribute
    {
        public string Type { get; }

        public string Name { get; }

        public int Order { get; }

        public EvmTypeAttribute(string typename, string name = null, int order = 1)
        {
            Type = typename;
            Name = name;
            Order = order;
        }
    }
}
