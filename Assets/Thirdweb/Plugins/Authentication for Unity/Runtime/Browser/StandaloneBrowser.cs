using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cdm.Authentication.Browser
{
    /// <summary>
    /// OAuth 2.0 verification browser that runs a local server and waits for a call with
    /// the authorization verification code.
    /// </summary>
    public class StandaloneBrowser : IBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        /// <summary>
        /// Gets or sets the close page response. This HTML response is shown to the user after redirection is done.
        /// </summary>
        public string closePageResponse { get; set; } = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";

        public async Task<BrowserResult> StartAsync(string loginUrl, string redirectUrl, CancellationToken cancellationToken = default)
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            using var httpListener = new HttpListener();

            try
            {
                redirectUrl = AddForwardSlashIfNecessary(redirectUrl);
                httpListener.Prefixes.Add(redirectUrl);
                httpListener.Start();
                httpListener.BeginGetContext(IncomingHttpRequest, httpListener);

                Application.OpenURL(loginUrl);

                return await _taskCompletionSource.Task;
            }
            finally
            {
                httpListener.Stop();
            }
        }

        private void IncomingHttpRequest(IAsyncResult result)
        {
            var httpListener = (HttpListener)result.AsyncState;
            var httpContext = httpListener.EndGetContext(result);
            var httpRequest = httpContext.Request;

            // Build a response to send an "ok" back to the browser for the user to see.
            var httpResponse = httpContext.Response;
            var buffer = System.Text.Encoding.UTF8.GetBytes(closePageResponse);

            // Send the output to the client browser.
            httpResponse.ContentLength64 = buffer.Length;
            var output = httpResponse.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

            _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, httpRequest.Url.ToString()));
        }

        /// <summary>
        /// Prefixes must end in a forward slash ("/")
        /// </summary>
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-7.0#remarks" />
        private string AddForwardSlashIfNecessary(string url)
        {
            string forwardSlash = "/";
            if (!url.EndsWith(forwardSlash))
            {
                url += forwardSlash;
            }

            return url;
        }
    }
}
