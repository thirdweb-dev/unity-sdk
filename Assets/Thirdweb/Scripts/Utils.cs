using UnityEngine;

namespace Thirdweb
{
    public class Utils
    {
        public static string[] ToJsonStringArray(params object[] args) {
            string[] stringArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
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
    }
}