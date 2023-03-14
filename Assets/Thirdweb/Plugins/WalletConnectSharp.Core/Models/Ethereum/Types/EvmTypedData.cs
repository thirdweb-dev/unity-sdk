using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Ethereum.Types
{
    /// <summary>
    /// EvmTypedData currently only supports primitive types.
    /// For lists, typles, and classes, use Nethereum for serialization.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EvmTypedData<TMessage>
    {
        /// <summary>
        /// An incomplete list of valid EVM types.
        /// </summary>
        public static readonly List<string> EvmTypes =
            new() { "address", "bool", "bytes", "bytes32", "string", "uint8", "uint16", "uint24", "uint32", "uint64", "uint128", "uint256", "int16", "int24", "int32", "int64", "int128", "int256", };

        [JsonProperty("types")]
        public Dictionary<string, EvmTypeInfo[]> Types { get; set; } = new Dictionary<string, EvmTypeInfo[]>();

        [JsonProperty("primaryType")]
        public string PrimaryType;

        [JsonProperty("domain")]
        public EIP712Domain Domain;

        [JsonProperty("message")]
        public TMessage Message;

        public EvmTypedData(TMessage data, EIP712Domain domain, string primaryType = null)
        {
            Message = data;
            Domain = domain;
            PrimaryType = string.IsNullOrWhiteSpace(primaryType) ? typeof(TMessage).Name : primaryType;

            AddTypeData(typeof(EIP712Domain));
            AddTypeData(typeof(TMessage));
        }

        /// <summary>
        /// Build EVM type infos from the specified type.
        /// Valid members must be expressed as public properties.
        /// </summary>
        public Dictionary<string, EvmTypeInfo> BuildTypeInfos(Type type)
        {
            var typeInfos = new Dictionary<string, EvmTypeInfo>();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            foreach (var property in type.GetProperties(bindingFlags))
            {
                var fieldType = property.PropertyType;
                var evmType = (EvmTypeAttribute)property.GetCustomAttribute(typeof(EvmTypeAttribute), true);
                var shouldIgnore = (EvmIgnoreAttribute)property.GetCustomAttribute(typeof(EvmIgnoreAttribute), true);

                if (evmType == null || shouldIgnore != null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(evmType.Type) || !EvmTypes.Contains(evmType.Type))
                {
                    throw new SerializationException($"Property {property.Name} has no valid EVM type mapping. Verify the [EvmType(...)] declaration type is a valid EVM type.");
                }

                var name = !string.IsNullOrWhiteSpace(evmType.Name) ? evmType.Name : property.Name;

                var typeInfo = new EvmTypeInfo(name, evmType.Type);
                _ = typeInfos.TryAdd(name, typeInfo);
            }

            return typeInfos;
        }

        public void AddTypeData(Type type)
        {
            var tname = type.Name;
            if (Types.ContainsKey(tname))
            {
                return;
            }

            var infos = BuildTypeInfos(type);
            var values = new List<EvmTypeInfo>();
            foreach (var info in infos)
            {
                values.Add(info.Value);
            }
            Types.Add(tname, values.ToArray());
        }
    }
}
