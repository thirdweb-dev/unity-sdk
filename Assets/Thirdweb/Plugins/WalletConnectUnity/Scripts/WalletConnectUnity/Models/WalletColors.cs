using System;
using Newtonsoft.Json;
using UnityEngine;

namespace WalletConnectUnity.Models
{
    public class WalletColors
    {
        public Color? PrimaryColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Primary))
                    return null;
                
                Color output;
                var result = ColorUtility.TryParseHtmlString(Primary, out output);
                if (!result)
                    throw new Exception("Invalid color: " + Primary);
                return output;
            }
        }

        public Color? SecondaryColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Secondary))
                    return null;
                
                Color output;
                var result = ColorUtility.TryParseHtmlString(Secondary, out output);
                if (!result)
                    throw new Exception("Invalid color: " + Secondary);
                return output;
            }
        }
        
        [JsonProperty("primary")]
        public string Primary;

        [JsonProperty("secondary")]
        public string Secondary;
    }
}