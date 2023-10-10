using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Cdm.Authentication.Utils
{
    public class UrlBuilder
    {
        private readonly UriBuilder _uriBuilder;
        private readonly NameValueCollection _query;
        
        private UrlBuilder(string url)
        {
            _uriBuilder = new UriBuilder(url);
            _query = HttpUtility.ParseQueryString("");
        }
        
        public static UrlBuilder New(string url)
        {
            return new UrlBuilder(url);
        }

        public UrlBuilder SetQueryParameters(Dictionary<string, string> parameters)
        {
            foreach (var p in parameters)
            {
                _query.Set(p.Key, p.Value);    
            }

            return this;
        }

        public override string ToString()
        {
            _uriBuilder.Query = _query.ToString();
            return _uriBuilder.Uri.ToString();
        }
    }
}