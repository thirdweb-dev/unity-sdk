using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core {

public static class Utils
{
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
            
        try
        {
            new Uri(url);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static bool IsValidRequestExpiry(long expiry, long min, long max)
    {
        return expiry <= max && expiry >= min;
    }

    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
    {
        TSource[] bucket = null;
        var count = 0;
        
        foreach (var item in source)
        {
            if (bucket == null)
                bucket = new TSource[size];

            bucket[count++] = item;
            if (count != size)
                continue;

            yield return bucket;

            bucket = null;
            count = 0;
        }

        if (bucket != null && count > 0)
            yield return bucket.Take(count).ToArray();
    }
}
}