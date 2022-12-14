using System.Collections.Generic;
using System.Linq;
using RLTY.SessionInfo;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace RLTY.Customisation
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    [AddComponentMenu("RLTY/Customisable/Selector")]
    public class CustomisableSelector : RLTYMonoBehaviour
    {
        #region F/P

        #region Serialize

        [SerializeField] private List<Customisable> customisables = new List<Customisable>();

        [SerializeField, ReadOnly] private Customisable actifCustomisable = null;
        [SerializeField, ReadOnly] private List<Customisable> actifCustomisables = new List<Customisable>();

        [SerializeField] private string customisableKey = "";

        [SerializeField] private bool useGameobjectName = false;
        [SerializeField, ShowIf("debug")] private bool showContainsChange = false;

        #endregion

        #region Private

        #endregion

        #region Public

        public string CustomisableKey => customisableKey;

        #endregion

        #endregion

        #region Unity Methods

        public override void Start()
        {
            base.Start();

            // UrlKeyValue tmp = new UrlKeyValue();
            // tmp.key = "CustomisableSelectorText";
            // tmp.value = "Ceci est un test";
            // Customize(tmp);
        }

        private void OnValidate()
        {
            CreateKey();

            CheckComponentsInChildren();
        }

        #endregion

        #region Custom Methods

        #region RLTYMonoBehaviour Override

        public override void EventHandlerRegister()
        {
        }

        public override void EventHandlerUnRegister()
        {
        }

        #endregion

        #region Private

        /// <summary>
        /// Check the children for any Processor not already add or remove if Processor are missing
        /// </summary>
        [Button("Check Customisables")]
        void CheckComponentsInChildren()
        {
            List<Customisable> tmpCustomisables = GetComponentsInChildren<Customisable>().ToList();

            foreach (Customisable customisable in tmpCustomisables)
            {
                if (!customisables.Contains(customisable))
                {
                    if (AddCustomisable(customisable))
                    {
                        if (debug && showContainsChange)
                            Debug.Log(
                                $"{customisable.gameObject.name} Added to {gameObject.name} {name} customisables list");
                    }
                    else
                    {
                        if (debug && showContainsChange)
                            Debug.Log(
                                $"{customisable.gameObject.name} couldn't be added to {gameObject.name} {name} customisables list");
                    }
                }

                RemoveNullObject();
            }
        }

        void CreateKey()
        {
            if (useGameobjectName)
                customisableKey = transform.name + "_" + gameObject.GetHashCode().ToString();
        }

        /// <summary>
        /// Remove null customisable from customisables List
        /// </summary>
        void RemoveNullObject()
        {
            foreach (Customisable customisable in customisables)
            {
                if (!customisable) customisables.Remove(customisable);
            }
        }

        /// <summary>
        /// Define the active customisable with the key and enabled his processor
        /// </summary>
        /// <param name="_key"></param>
        private void SelectCustomisable(string _key)
        {
            foreach (Customisable customisable in customisables)
            {
                string keyToCompare = _key + customisable.gameObject.GetHashCode();
                if (keyToCompare.Contains(customisable.key))
                {
                    actifCustomisables.Add(customisable);
                    customisable.gameObject.SetActive(true);
                }
            }

            foreach (Customisable customisable in customisables)
            {
                if (!actifCustomisables.Contains(customisable))
                    customisable.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Add a customisable in customisables List
        /// </summary>
        /// <param name="_customisable">Customisable to add</param>
        /// <returns>True if customisable add successfully, false if not add</returns>
        public bool AddCustomisable(Customisable _customisable)
        {
            if (customisables.Contains(_customisable))
                return false;
            customisables.Add(_customisable);
            return true;
        }

        /// <summary>
        /// Cycle to select the customisable then active it
        /// </summary>
        /// <param name="_value"></param>
        public void Customize(KeyValueBase _value)
        {
            SelectCustomisable(_value.key);
            if (actifCustomisables.Count > 0)
                actifCustomisables[^1].Customize(_value);
        }

        #endregion

        #endregion
    }
}