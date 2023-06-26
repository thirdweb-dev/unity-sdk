using System;

namespace WalletConnectSharp.Crypto.Encoder
{
    internal class Codec
    {
        public string Name { get; }
        public string Prefix { get; }
        public Func<byte[], string> Encoder { get; }
        public Func<string, byte[]> Decoder { get; }

        public Codec(string name, string prefix, BaseX baseX) : this(name, prefix, baseX.Encode, baseX.Decode)
        {
            
        }

        public Codec(string name, string prefix, Func<byte[], string> encoder, Func<string, byte[]> decoder)
        {
            this.Name = name;
            this.Prefix = prefix;
            this.Encoder = encoder;
            this.Decoder = decoder;
        }

        public string Encode(byte[] bytes)
        {
            return $"{Encoder(bytes)}";
        }

        public byte[] Decode(string source)
        {
            if (source[0] != Prefix[0])
            {
                source = Prefix + source;
            }

            return Decoder(source.Substring(1));
        }
    }
}
