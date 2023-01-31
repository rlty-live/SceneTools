using System;

using RLTY.Customisation;

namespace RLTY.SessionInfo
{
    public class SessionInfoManagerHandlerData
    {
        /// <summary>
        /// Do not listen to this event otherwise you will override SessionInfoManager
        /// </summary>
        public static event Func<SceneDescription> OnGetSceneDescription;

        /// <summary>
        /// Get the scene description
        /// </summary>
        /// <returns></returns>
        public static SceneDescription GetSceneDescription() => OnGetSceneDescription?.Invoke();

        /// <summary>
        /// Do not listen to this event otherwise you will override SessionInfoManager
        /// </summary>
        public static event Func<string> OnGetUserName;
        public static string GetUserName() => OnGetUserName?.Invoke();
        
        public static event Func<string> OnGetEventName;
        public static string GetEventName() => OnGetEventName?.Invoke();

        public static event Action OnServerReady;
        public static void ServerReady() => OnServerReady?.Invoke();

        public static event Action OnClientReady;
        public static void ClientReady() => OnClientReady?.Invoke();
        
        public static event Action<float> OnAssetLoadProgress;
        public static void AssetLoadProgress(float p) => OnAssetLoadProgress?.Invoke(p);

        public static event Action OnStartAnalyzeVoice;
        public static void StartAnalyzeVoice() => OnStartAnalyzeVoice?.Invoke();

        public static event Action<string> OnUserEnterConferenceStage;
        public static void UserEnterConferenceStage(string stageId) => OnUserEnterConferenceStage?.Invoke(stageId);

        public static event Action<string> OnUserExitConferenceStage;
        public static void UserExitConferenceStage(string stageId) => OnUserExitConferenceStage?.Invoke(stageId);

        public static event Action<string> OnUserDonation;
        public static void UserDonation(string walletId) => OnUserDonation?.Invoke(walletId);

        public static event Action<string> OnWeb3Transaction;
        public static void Web3Transaction(string data) => OnWeb3Transaction?.Invoke(data);

        public static event Action<string> OnOpenTypeForm;
        public static void OpenTypeForm(string typeformId) => OnOpenTypeForm?.Invoke(typeformId);



        public static event Action<string, Action<bool>> OnPlayerValidate;

        public static void ValidatePlayer(string playerSessionId, Action<bool> callback) => OnPlayerValidate?.Invoke(playerSessionId, callback);

        public static event Action<string, Action<bool>> OnPlayerDisconnect;

        public static void DisconnectPlayer(string playerSessionId, Action<bool> callback) => OnPlayerDisconnect?.Invoke(playerSessionId, callback);

        public static event Action<Action<bool>> OnServerClose;

        public static void ServerClose(Action<bool> callback) => OnServerClose?.Invoke(callback);

    }

    /// <summary>
    /// All information related to the game session (this is useful only on the client side)
    /// </summary>
    [System.Serializable]
    public class GameLiftGameSession
    {
        public string PlayerSessionId = "sessionId";
        public string GameSessionId = "gameSessionId";
        public string DnsName = "localhost";
        public string Voice = "none"; //agora or chime
        public int Port = 7777;
    }
}
