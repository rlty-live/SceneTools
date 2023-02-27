using System.Collections;
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
        private string sceneToolsVersion;

        [Title("Organizing")]
        [Space(5)]
        [DetailedInfoBox("How to", howTo, InfoMessageType.Info)]
        public List<string> groupLabel = new List<string>();
        public List<string> groups = new List<string>();
        public List<string> sections = new List<string>();

        [Title("Sorting")]
        public List<Customisable> customisablesInScene = new List<Customisable>();

        //Set in Reset()
        Texture2D customisableClassification;

        [SerializeField]
        private const string howTo =
    "1) Reorder those lists to change their displaying order on the website \n" +
    "2) You can add and set groups on any Customizable component \n" +
            "see documentation for more information.";

        [PropertyOrder(40)]
        [SerializeField, HorizontalGroup("selector", Title = "Tools"), LabelWidth(100)]
        private CustomisableType customisables;
        #endregion

        #region ThreadSafe Singleton
        /// Source: https://refactoring.guru/design-patterns/singleton/csharp/example#example-1
        /// This Singleton implementation is called "double check lock". It is safe in multithreaded environment and provides lazy initialization for the Singleton object.
        private CustomisationManager() { }
        private static CustomisationManager _instance;

        /// We now have a lock object that will be used to synchronize threads during first access to the Singleton.
        private static readonly object _lock = new object();

        public static CustomisationManager GetInstance()
        {
            ///This conditional is needed to prevent threads stumbling over the lock once the instance is ready.
            if (_instance == null)
            {
                /// Now, imagine that the program has just been launched. 
                /// Since there's no Singleton instance yet, multiple threads can simultaneously pass the previous conditional and reach this point almost at the same time. 
                /// The first of them will acquire lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    /// The first thread to acquire the lock, reaches this conditional, goes inside and creates the Singleton instance.
                    /// Once it leaves the lock block, a thread that might have been waiting for the lock release may then enter this section.
                    /// But since the Singleton field is already initialized, the thread won't create a new object.
                    if (_instance == null)
                        _instance = new CustomisationManager();
                }
            }
            return _instance;
        }
        #endregion

        #region EditorOnly logic
#if UNITY_EDITOR

        [Button]
        public void OpenDocumentation() => Application.OpenURL("https://rlty.notion.site/How-to-set-descriptions-for-customizables-15f2ae881601470084454b994f8c1cbf");

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

            GetCustomisableList();
        }

        public void Reset()
        {
            customisableClassification = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.live.rlty.scenetools/Docs/Customisables/classification.png", typeof(Texture2D));
        }

        [ExecuteInEditMode]
        public void GetCustomisableList()
        {
            //Check if a previous List exists
            if (customisablesInScene != null)
            {
                //If there are elements in it
                if (customisablesInScene.Count > 0)
                {
                    Customisable[] tempCustomisablesInScene = FindObjectsOfType<Customisable>();

                    //Only add the new ones at the end
                    foreach (Customisable custo in tempCustomisablesInScene)
                    {
                        if (customisablesInScene.Contains(custo)) continue;
                        else customisablesInScene.Add(custo);
                    }

                    tempCustomisablesInScene = null;

                    JLogBase.Log("Updated customisable List", this);
                }
                //if not create new list
                else
                {
                    customisablesInScene = new List<Customisable>();
                    Customisable[] tempCustomisablesInScene = FindObjectsOfType<Customisable>();

                    foreach (Customisable custo in tempCustomisablesInScene)
                        customisablesInScene.Add(custo);

                    tempCustomisablesInScene = null;
                    JLogBase.Log("Created new List", this);
                }
            }
            //if not create new list
            else
            {
                customisablesInScene = new List<Customisable>();
                Customisable[] tempCustomisablesInScene = FindObjectsOfType<Customisable>();

                foreach (Customisable custo in tempCustomisablesInScene)
                    customisablesInScene.Add(custo);

                tempCustomisablesInScene = null;

                JLogBase.Log("Created new List", this);
            }

            foreach (Customisable custo in customisablesInScene)
                custo.UpdateCommentary();
        }
#endif
        #endregion

        #region Runtime logic

        public void Awake()
        {
            GetInstance();
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

