using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Thirdweb
{
    public class Utils
    {

        public const string AddressZero = "0x0000000000000000000000000000000000000000";
        public const string NativeTokenAddress = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";

        public static string[] ToJsonStringArray(params object[] args) {
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

        public static string ToJson(object obj) {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string ToBytes32HexString(byte[] bytes)
        {
            var hex = new StringBuilder(64);
            foreach(var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return "0x" + hex.ToString().PadLeft(64, '0');
        }

        public static long UnixTimeNowMs()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long) timeSpan.TotalMilliseconds;
        }
    }
}