using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        private readonly ThirdwebSDK _sdk;

        public ThirdwebPay(ThirdwebSDK sdk)
        {
            _sdk = sdk;
        }
    }
}
