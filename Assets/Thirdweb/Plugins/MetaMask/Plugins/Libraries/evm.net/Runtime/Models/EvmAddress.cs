namespace evm.net.Models
{
    public class EvmAddress : IConvertableType
    {
        public string Value { get; set; }

        public EvmAddress(string value)
        {
            // TODO Add validation

            Value = value;
        }

#nullable enable

        public static implicit operator string?(EvmAddress d) => d?.Value;

#nullable disable

        public static implicit operator EvmAddress(string b) => new EvmAddress(b);

        public override string ToString() => Value.StartsWith("0x") ? Value : $"0x{Value}";

        public object Convert()
        {
            return ToString();
        }
    }
}
