using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace RLTY.SessionInfo
{
    public class SessionInfoManagerHandlerData
    {
        /// <summary>
        /// Do not listen to this event otherwise you will override SessionInfoManager
        /// </summary>
        public static event Func<GameLiftGameSession> OnGetGameLiftGameSession;
        /// <summary>
        /// 
        /// </summary>
        /// <returns>GameLiftGameSession</returns>
        public static GameLiftGameSession GetGameLiftGameSession() => OnGetGameLiftGameSession?.Invoke();

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

        public static event Action<Action> OnServerClose;

        public static void ServerClose(Action callback) => OnServerClose?.Invoke(callback);

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

    /// <summary>
    /// All information needed to build the scene on the client side
    /// </summary>
    [System.Serializable]
    public class SceneDescription
    {
        /// <summary>
        /// URL of the assetbundle to load on the server side
        /// </summary>
        public string assetbundleServer;
        /// <summary>
        /// URL of the assetbundle to load on the client side
        /// </summary>
        public string assetbundleClient;

        public List<string> deactivatorKey= new List<string>();

        public List<KeyValueBase> text = new List<KeyValueBase>();
        public List<ColorKeyValue> color= new List<ColorKeyValue>();
        public List<UrlKeyValue> image = new List<UrlKeyValue>();
        public List<UrlKeyValue> audioClips = new List<UrlKeyValue>();
        public List<UrlKeyValue> audioFeeds = new List<UrlKeyValue>();
        public List<UrlKeyValue> videoFeed = new List<UrlKeyValue>();
        public List<UrlKeyValue> externalPageUrl = new List<UrlKeyValue>();
        public List<KeyValueBase> donationBox = new List<KeyValueBase>();
        public List<KeyValueBase> typeForm = new List<KeyValueBase>();
        public List<KeyValueBase> web3Transaction = new List<KeyValueBase>();

        public void SaveAsJSON()
        {
            string dir = Application.dataPath + "/../../StreamingAssets";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string path = dir + "/sceneDescription.json";
            File.WriteAllText(dir, JsonConvert.SerializeObject(this, Formatting.Indented));
            Debug.Log("SceneDescription saved to " + dir);
        }
    }

    [Serializable]
    public class ColorKeyValue : KeyValueBase
    {
        public Color32 GetColor()
        {
            string[] parsedString = value.Split(",");

            if (parsedString.Length > 3) Debug.Log("Wrong string input, requires Vector4 [0,255]");

            for (int i = 0; i < parsedString.Length; i++)
            {
                parsedString[i] = parsedString[i].Replace(" ", "");
            }

            parsedString[0] = parsedString[0].Replace("[", "");
            parsedString[parsedString.Length - 1] = parsedString[parsedString.Length - 1].Replace("]", "");

            Vector4 parsedColor = new Vector4();

            parsedColor.x = int.Parse(parsedString[0]);
            parsedColor.y = int.Parse(parsedString[1]);
            parsedColor.z = int.Parse(parsedString[2]);
            parsedColor.w = int.Parse(parsedString[3]);

            return new Color32((byte)parsedColor.x, (byte)parsedColor.y, (byte)parsedColor.z, (byte)parsedColor.w);
        }
    }

    [Serializable]
    public class UrlKeyValue : KeyValueBase
    {
    }

    [Serializable]
    public class KeyValueBase
    {
        public string key;
        [TextArea]
        public string value;
    }

    #region Data
    [System.Serializable]
    public class KeyValueObject : KeyValueBase
    {
        [ReadOnly]
        public UnityEngine.Object downloadedAsset;

        /// <summary>
        ///  Constructor for KeyValueObject, Holds the reference of the downloaded object from keyValue
        /// </summary>
        /// <param name="_keyValue">name and URL (if applicable) of the target asset</param>
        /// <param name="_asset">Downloaded object</param>
        public KeyValueObject(KeyValueBase _keyValue, UnityEngine.Object _asset)
        {
            key = _keyValue.key;
            value = _keyValue.value;
            downloadedAsset = _asset;
        }
    }

    [System.Serializable]
    public class KeyValueTexture : KeyValueObject
    {
        public Texture texture;

        public KeyValueTexture(KeyValueBase _keyValue, UnityEngine.Object _asset) : base(_keyValue, _asset)
        {
            texture = (Texture)_asset;
        }
    }

    [System.Serializable]
    public class KeyValueAudio : KeyValueObject
    {
        public AudioClip clip;

        public KeyValueAudio(KeyValueBase _keyValue, UnityEngine.Object _asset) : base(_keyValue, _asset)
        {
            clip = (AudioClip)_asset;
        }
    }

    //WIP code, to modify(SessionDescription lacks conversion from vector4 to Color32 as of the writing of this snippet
    //[System.Serializable]
    //public class KeyValueColor : KeyValueObject
    //{
    //    [SerializeField]
    //    private ColorKeyValue originalValue;
    //    public Color32 convertedColor;

    //        public KeyValueColor(ColorKeyValue _keyValue, Object _asset): base(_keyValue, _asset)
    //        {
    //            int r;
    //            int g;
    //            int b;
    //            int a;

    //            _keyValue.value

    //originalValue = new Color32((byte) originalValue.key)
    //        }
    [System.Serializable]
    public class ProcessedSceneDescription
    {
        [SerializeField]
        public SceneDescription sceneDescription;
        public List<KeyValueObject> downloadedObjects;
        [SuffixLabel("mb"), ReadOnly]
        public int totalSize;

        public ProcessedSceneDescription(SceneDescription _sceneDescription)
        {
            sceneDescription = _sceneDescription;
            downloadedObjects = new List<KeyValueObject>();
        }
    }
    #endregion
}
