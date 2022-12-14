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

        #region Serialize

        [SerializeField] private Button button = null;
        [SerializeField] private string url = "";

        #endregion

        #region Private

        #endregion

        #region Public

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

        #endregion

        public override Component FindComponent(Component existingTarget)
        {
            Component target = null;
            Button button = GetComponentInChildren<Button>();

            if (!button)
            {
                if (debug)
                    Debug.LogWarning("No Button found in children" + commonWarning, this);
            }
            else
                target = button;
            return target;
        }

        public override void Customize(Component target, KeyValueBase keyValue)
        {
            SetURL(keyValue.value);

        }

        #region Unity Methods

        public override void Start()
        {
            base.Start();

            if (!button) return;

            button.onClick.AddListener(() => OpenNewInternetPage(url));
        }

        #endregion

        #region Custom Methods

        #region Private

        #endregion

        #region Public
        /// <summary>
        /// Open an internet page with the URL given
        /// </summary>
        /// <param name="_url">URL of the page web you want to open</param>
        public void OpenNewInternetPage(string _url)
        {
            if (!IsValid) return;

            Application.OpenURL(_url);
        }

        /// <summary>
        /// Set the url target to open when button clicked
        /// </summary>
        /// <param name="_url">URL target</param>
        public void SetURL(string _url)
        {
            url = _url;
        }

        #endregion

        #endregion
    }
}