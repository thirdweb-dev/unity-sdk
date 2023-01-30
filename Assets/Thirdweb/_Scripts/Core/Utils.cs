using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb
{
    public static class Utils
    {

        public const string AddressZero = "0x0000000000000000000000000000000000000000";
        public const string NativeTokenAddress = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        public const double DECIMALS_18 = 1000000000000000000;

        public static string[] ToJsonStringArray(params object[] args)
        {
            List<string> stringArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                // if value type, convert to string otherwise serialize to json
                if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs.Add(args[i].ToString());
                }
                else
                {
                    stringArgs.Add(Utils.ToJson(args[i]));
                }
            }
            return stringArgs.ToArray();
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string ToBytes32HexString(byte[] bytes)
        {
            var hex = new StringBuilder(64);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return "0x" + hex.ToString().PadLeft(64, '0');
        }

        public static long UnixTimeNowMs()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        public static string ToWei(this string eth)
        {
            double ethDouble = 0;
            if (!double.TryParse(eth, out ethDouble))
                throw new ArgumentException("Invalid eth value.");
            BigInteger wei = (BigInteger)(ethDouble * DECIMALS_18);
            return wei.ToString();
        }

        public static string ToEth(this string wei, int decimalsToDisplay = 4)
        {
            return FormatERC20(wei, decimalsToDisplay);
        }

        public static string FormatERC20(this string wei, int decimalsToDisplay = 4, int decimals = 18)
        {
            BigInteger weiBigInt = 0;
            if (!BigInteger.TryParse(wei, out weiBigInt))
                throw new ArgumentException("Invalid wei value.");
            double eth = (double)weiBigInt / Math.Pow(10.0, decimals);
            string format = "#,0";
            if (decimalsToDisplay > 0)
                format += ".";
            for (int i = 0; i < decimalsToDisplay; i++)
                format += "#";
            return eth.ToString(format);
        }
    }
}