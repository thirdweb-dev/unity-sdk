using System;
using System.Text;

namespace link.magic.unity.sdk.Utility
{
    internal class MagicUtility
    {
        public static string BtoA(string str)
        {
            var optionsInBytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(optionsInBytes);
        }
    }
}