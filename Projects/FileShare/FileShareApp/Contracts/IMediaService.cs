using FileShareApp.Helpers;

namespace FileShareApp.Contracts
{
    public interface IMediaService
    {
        event EventHandler<MediaEventArgs> OnMediaAssetLoaded;

        bool IsLoading { get; }

        Task<IList<MediaAssest>> RetrieveMediaAssetsAsync(CancellationToken? token = null);

        Task<string> StoreProfileImage(string path);

        Task<string> GetImageWithCamera();
    }
}
