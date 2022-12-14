using UnityEngine.Events;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    public static class AssetDownloaderManagerHandlerData
    {
        public static UnityAction<SessionInfo.UrlKeyValue> OnDownloadRequested;
        public static void RequestDownload(SessionInfo.UrlKeyValue urlKeyValue) => OnDownloadRequested?.Invoke(urlKeyValue);

        public static UnityAction<KeyValueObject> OnDownloadSuccess;
        public static void DownloadSuccess(KeyValueObject keyValueObject) => OnDownloadSuccess?.Invoke(keyValueObject);


        public static UnityAction<SessionInfo.UrlKeyValue> OnDownloadFailed;
        public static void DownloadFailed(SessionInfo.UrlKeyValue urlKeyValue) => OnDownloadFailed?.Invoke(urlKeyValue);


        public static UnityAction<ProcessedSceneDescription> OnAllDownloadFinished;
        public static void AllDownloadFinished(ProcessedSceneDescription downloadedAssets) => OnAllDownloadFinished?.Invoke(downloadedAssets);
    }
}
