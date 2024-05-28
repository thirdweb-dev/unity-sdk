namespace WalletConnectUnity.Core.Utils
{
    public static class StringExtensions
    {
        public static string ToHex(this string str)
        {
            return $"0x{int.Parse(str):X}";
        }
    }
}