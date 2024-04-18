using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class FiatCurrenciesResponse
    {
        [JsonProperty("result")]
        public FiatCurrenciesResult Result { get; set; }
    }

    public class FiatCurrenciesResult
    {
        [JsonProperty("fiatCurrencies")]
        public List<string> FiatCurrencies { get; set; }
    }
}
