using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor;
#endif

namespace RLTY.Customisation
{
    [HideMonoScript]
    [HelpURL("https://www.youtube.com/watch?v=-AXetJvTfU0"), AddComponentMenu("RLTY/Customisable/Managers/Customizer")]
    public class CustomisationManager : RLTYMonoBehaviour
    {
        #region Global variables
        [SerializeField, ReadOnly]
        [DetailedInfoBox("How to", howTo, InfoMessageType.Info)]
        private string sceneToolsVersion;

        [Title("Sorting")]
        [InfoBox("groupLabel ...")]
        public List<string> groupLabel;
        [InfoBox("Groups are displayed in the same labeless box on the website")]
        public List<string> groups;
        [InfoBox("Sections are labeled boxes that encompasses groups")]
        public List<string> sections;

        [SerializeField]
        private const string howTo =
    "1) Reorder those lists to change their displaying order on the website \n" +
    "2) You can add and set groups on any Customizable component";



        [PropertyOrder(40)]
        [SerializeField, HorizontalGroup("selector", Title = "Tools"), LabelWidth(100)]
        private CustomisableType customisables;


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

        public void OnValidate()
        {
            foreach (PackageInfo packageInfo in PackageInfo.GetAllRegisteredPackages())
            {
                if (packageInfo.name == "live.rlty.scenetools")
                    sceneToolsVersion = packageInfo.version;
            }
        }
#endif
        #endregion

        #region Runtime logic
        private static CustomisationManager _instance;

        public void Awake()
        {
            if (_instance != null)
            {
                enabled = false;
                return;
            }
            _instance = this;
            LogPackageVersion();
        }

        public void LogPackageVersion()
        {
            if (sceneToolsVersion != null)
                JLog("RLTY SceneTools package version: " + sceneToolsVersion);
        }

        /// <summary>
        /// Gathers all customisables, check compatibility with SceneDescription stored in database, 
        /// and starts customization
        /// </summary>
        /// <param name="sceneDescription">The configuration file that lists all customisation keys and values for this building and this event</param>
        public void CustomizeScene(SceneDescription sceneDescription)
        {
            Customisable[] fullList = FindObjectsOfType<Customisable>();
            JLog("Starting Customization, customisable count=" + fullList.Length);
            CustomisationManagerHandlerData.CustomizationStarted();
            foreach (CustomisableTypeEntry entry in sceneDescription.entries)
            {
                CustomisableType type = entry.Type;
                foreach (KeyValueBase k in entry.keyPairs)
                {
                    if (type == CustomisableType.Invalid)
                        JLogError("Invalid key type in scene description: key=" + k.key + " value=" + k.value + " type=" + entry.type);
                    else
                        SearchAndCustomize(type, fullList, k);
                }
            }

#if UNITY_EDITOR
            JLog("\n <b>All gameobject that does not receive a value for customisation will be deactivated," +
                "that can happen for two reasons: </b> \n" +
                "1) It's SceneDescription key is not up to date and need to be regenerated <i>(Toolbar/RLTY/CreateSceneDescription)</i>\n" +
                "2) No value as being assigned to its key, look for the corresponding <i>SessionConfig</i> ScriptableObject in the asset folder\n\n");
#else
            JLog("All gameobject that does not receive a value for customisation will be deactivated, it means that no value has been set in the Event Configuration tab of the event");
#endif

            JLog("Finished Customization");
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
                JLogError("No customisable found for key=" + k.key + " type=" + type);
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
}