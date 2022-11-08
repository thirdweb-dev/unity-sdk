using System;
using System.Text;
using UnityEngine;

namespace Thirdweb
{
    public class Utils
    {

        public static string AddressZero = "0x0000000000000000000000000000000000000000";

        public static string[] ToJsonStringArray(params object[] args) {
            string[] stringArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Log("Type" + args[i].GetType());
                // if value type, convert to string otherwise serialize to json
                if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs[i] = args[i].ToString();
                }
                else
                {
                    stringArgs[i] = JsonUtility.ToJson(args[i]);
                }
            }
            return stringArgs;
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