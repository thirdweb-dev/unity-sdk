namespace MetaMask.Samples.Main.Scripts.Utils
{
    public static class ExtensionMethods
    {
        public static string RepeatLinq(this string text, uint n)
        {
            return string.Concat(System.Linq.Enumerable.Repeat(text, (int)n));
        }
    }
}