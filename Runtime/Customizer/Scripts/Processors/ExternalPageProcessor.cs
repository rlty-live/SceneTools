using RLTY.Customisation;
using RLTY.SessionInfo;
using UnityEngine;
using UnityEngine.UI;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Customisable/Processors/External web page")]
    public class ExternalPageProcessor : Processor
    {
        #region F/P

        [SerializeField] private Button button = null;
        [SerializeField] private string url = "";

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(url)) return false;

                if (!button)
                {
                    if (!TryGetComponent(out Button but))
                        return false;
                    button = but;
                }

                return true;
            }
        }

        #endregion

        public override Component FindComponent()
        {
            if (!button)
                if (debug)
                    Debug.LogWarning("No Button found in children" + commonWarning, this);
            return button;
        }

        public override void Customize(KeyValueBase keyValue) => SetURL(keyValue.value);
        /// <summary>
        /// Set the url target to open when button clicked
        /// </summary>
        /// <param name="_url">URL target</param>
        public void SetURL(string _url) => url = _url;

        public override void Start()
        {
            base.Start();
            if (!button) return;
            button.onClick.AddListener(() => OpenNewInternetPage(url));
        }

        /// <summary>
        /// Open an internet page with the given URL given
        /// </summary>
        /// <param name="_url">URL of the page web you want to open</param>
        public void OpenNewInternetPage(string _url)
        {
            if (!IsValid) return;
            Application.OpenURL(_url);
            if (debug) 
                Debug.Log("Trying to open external url: " + _url, this);
        }

        public void OpenNewInternetPage()
        {
            if (!IsValid) return;
            Application.OpenURL(url);
            if (debug)
                Debug.Log("Trying to open external url: " + url, this);
        }
    }
}