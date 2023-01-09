using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RLTY.SessionInfo;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Customisation
{
    [HideMonoScript]
    [HelpURL("https://www.youtube.com/watch?v=-AXetJvTfU0"), AddComponentMenu("RLTY/Customisable/Managers/Customizer")]
    public class CustomisationManager : RLTYMonoBehaviour
    {
        #region Global variables

        [PropertyOrder(40)] [SerializeField, HorizontalGroup("selector", Title = "Tools"), LabelWidth(100)]
        private CustomisableType customisables;
        private const string howTo =
            "To easily navigate through the list add a second inspector side-by-side and lock this one. " +
            "Then navigate in the list by double clicking any Customisable List";
        #endregion

        #region EditorOnly logic
#if UNITY_EDITOR

        /// <summary>
        /// Select all Customisable corresponding to CustomisablesToSelect in Hierarchy
        /// </summary>
        [Button("Select"), HorizontalGroup("selector")]
        public void SelectAll()
        {
            Customisable[] list = FindObjectsOfType<Customisable>();
            List<GameObject> gos = new List<GameObject>();
            foreach (Customisable c in list)
            {
                if (c.type == customisables)
                    gos.Add(c.gameObject);
            }
            Selection.objects = gos.ToArray();
        }
        
        #endif

        #endregion

        #region Runtime logic

        /// <summary>
        /// Asks every loaded and activated customisable to modify its correponding Components
        /// </summary>
        /// <param name="sceneDescription">The configuration file that lists all customisation keys and values for this building and this event</param>
        public void CustomizeScene(SceneDescription sceneDescription)
        {
            JLog("Starting Customization from " + sceneDescription, this);

            Customisable[] fullList = FindObjectsOfType<Customisable>();

            //Debug.Log("to do: reintegrate deactivator");

            CustomisationManagerHandlerData.CustomizationStarted();
            foreach (CustomisableTypeEntry entry in sceneDescription.entries)
            {
                CustomisableType type = entry.Type;
                foreach (KeyValueBase k in entry.keyPairs)
                {
                    if (type == CustomisableType.Invalid)
                        Debug.LogError("Invalid key type in scene description: key=" + k.key+" value="+ k.value+" type="+entry.type);
                    else
                        SearchAndCustomize(type, fullList, k);
                }
            }

            JLog("Finished Customization from " + sceneDescription, this);
            CustomisationManagerHandlerData.CustomisationFinished();
        }

        void SearchAndCustomize(CustomisableType type, Customisable[] fullList, KeyValueBase k)
        {
            string foundKeys = null;
            bool found = false;
            foreach (Customisable customisable in fullList)
                if (customisable.type == type && customisable.key.Contains(k.key))
                {
                    customisable.Customize(k);
                    found = true;
                    if (debug)
                    {
                        if (foundKeys != null)
                            foundKeys += ",";
                        foundKeys += customisable.key;
                    }
                }
            if (!found)
                Debug.LogError("No customisable found for key=" + k.key+" type="+type);
            else JLog(k.key + " was found in: " + foundKeys);
        }

        #endregion

        #region Observer Pattern

        public override void EventHandlerRegister()
        {
            AssetDownloaderManagerHandlerData.OnAllDownloadFinished += CustomizeScene;
            //AssetDownloaderManagerHandlerData.OnDownloadSuccess += PrepareCustomization;
        }

        public override void EventHandlerUnRegister()
        {
            AssetDownloaderManagerHandlerData.OnAllDownloadFinished -= CustomizeScene;
            //AssetDownloaderManagerHandlerData.OnDownloadSuccess -= PrepareCustomization;
        }

        #endregion
    }

    #region Data

    /// <summary>
    /// Editor and runtime oriented list of customisable
    /// </summary>
    public class CustomisableList : SerializedScriptableObject
    {
        [HideInInspector]
        //public string name;
        public CustomisableType type;

        public int dictionnaryLength;

        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, Customisable> customisablesDictionnary = new Dictionary<string, Customisable>();
    }

    #endregion
}