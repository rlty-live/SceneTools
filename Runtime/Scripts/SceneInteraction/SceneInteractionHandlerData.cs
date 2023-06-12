using System;

using RLTY.Customisation;

namespace RLTY.SessionInfo
{
    public class SceneInteractionHandlerData
    {
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
        
        public static event Action<string> OnOpenIframe;
        public static void OpenIframe(string iframeURL) => OnOpenIframe?.Invoke(iframeURL);

    }
}
