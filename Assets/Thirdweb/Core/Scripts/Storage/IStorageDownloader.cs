using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb
{
    public interface IStorageDownloader
    {
        Task<T> DownloadText<T>(string textURI);
        Task<Sprite> DownloadImage(string imageURI);
    }
}
