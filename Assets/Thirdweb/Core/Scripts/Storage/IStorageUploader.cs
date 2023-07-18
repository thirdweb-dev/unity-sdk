using System.Threading.Tasks;

namespace Thirdweb
{
    public interface IStorageUploader
    {
        Task<IPFSUploadResult> UploadText(string text);
        Task<IPFSUploadResult> UploadFromPath(string path);
    }
}
