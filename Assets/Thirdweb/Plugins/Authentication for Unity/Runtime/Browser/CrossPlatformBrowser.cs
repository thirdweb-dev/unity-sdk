using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cdm.Authentication.Browser
{
    public class CrossPlatformBrowser : IBrowser
    {
        public readonly Dictionary<RuntimePlatform, IBrowser> _platformBrowsers = 
            new Dictionary<RuntimePlatform, IBrowser>();
        
        public IDictionary<RuntimePlatform, IBrowser> platformBrowsers => _platformBrowsers;

        public async Task<BrowserResult> StartAsync(
            string loginUrl, string redirectUrl, CancellationToken cancellationToken = default)
        {
            var browser = platformBrowsers.FirstOrDefault(x => x.Key == Application.platform).Value;
            if (browser == null)
                throw new NotSupportedException($"There is no browser found for '{Application.platform}' platform.");

            return await browser.StartAsync(loginUrl, redirectUrl, cancellationToken);
        }
    }
}