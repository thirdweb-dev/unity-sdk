using System;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class BuyHistoryResponse
    {
        [JsonProperty("result")]
        public BuyHistoryResult Result { get; set; }
    }

    public class BuyHistoryResult
    {
        [JsonProperty("walletAddress")]
        public string WalletAddress { get; set; }

        [JsonProperty("page")]
        public List<HistoryPage> Page { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    public class HistoryPage
    {
        [JsonProperty("buyWithCryptoStatus")]
        public BuyWithCryptoStatusResult BuyWithCryptoStatus;

        [JsonProperty("buyWithFiatStatus")]
        public BuyWithFiatStatusResult BuyWithFiatStatus;
    }
}
