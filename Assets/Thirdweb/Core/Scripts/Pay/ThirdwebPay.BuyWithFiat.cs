using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Buy crypto with fiat using the onramp link from the quote and get a quote ID to poll for the onramp status.
        /// </summary>
        /// <param name="buyWithFiatQuote">Quote containing onramp details</param>
        /// <param name="sdk">Optional SDK instance, defaults to ThirdwebManager instance</param>
        /// <returns>Quote ID to poll for the onramp status</returns>
        public string BuyWithFiat(BuyWithFiatQuoteResult buyWithFiatQuote)
        {
            if (string.IsNullOrEmpty(buyWithFiatQuote.OnRampLink))
            {
                throw new Exception("OnRampLink is required to buy with fiat.");
            }

            var onRampLink = buyWithFiatQuote.OnRampLink;

            Application.OpenURL(onRampLink);

            return buyWithFiatQuote.IntentId;
        }
    }
}
