using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

#if UNITY_EDITOR
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor;
#endif

namespace RLTY.Customisation
{
    [DisallowMultipleComponent]
    [HideMonoScript]
    [HelpURL(docURL), AddComponentMenu("RLTY/Customisable/Managers/Customisation Manager")]
    public class CustomisationManager : RLTYMonoBehaviour
    {
        #region Global variables

        [SerializeField, ReadOnly] private string sceneToolsVersion;

        [Title("Organizing")]
        [Space(5)]
        [DetailedInfoBox("warning", warning, InfoMessageType.Warning)]
        public List<string> groupLabels = new List<string>();
        public List<string> sections = new List<string>();
        public List<string> groups = new List<string>();

        [Title("Sorting")]
        [SerializeField]
        public List<Customisable> activeCustomisablesInScene = new List<Customisable>();
        public List<Customisable> customisablesInScene = new List<Customisable>();

        [SerializeField]
        private const string warning = "Renaming a category won't have an effect on customisable previously setup until you have refreshed them";

        private const string ordering =
       "1) Reorder those lists to change their displaying order on the website \n" +
       "2) You can add and set groups on any Customizable component \n" +
       "see documentation for more information.";

        [PropertyOrder(40)]
        [SerializeField, HorizontalGroup("selector", Title = "Tools"), LabelWidth(100)]
        private CustomisableType customisables;

        [ReadOnly, SerializeField] private List<Customisable> uncustomizedCustomisables;

        private const string docURL = "https://rlty.notion.site/How-to-set-descriptions-for-customizables-15f2ae881601470084454b994f8c1cbf";

        #endregion

        #region EditorOnly logic

#if UNITY_EDITOR

        [Button]
        public void OpenDocumentation() => Application.OpenURL(docURL);

        /// <summary>
        /// Select all Customisable corresponding to CustomisablesToSelect in Hierarchy
        /// </summary>
        [Button("Select"), HorizontalGroup("TypeSelector")]
        public void SelectAll(CustomisableType type)
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

        //If i don't find a way to automatically refresh a customisable selected group/labelgroup etc after renaming, i can make them selectable by those criterias for rapid change
        //[Button("Select"), HorizontalGroup("")]
        //public void SelectAll(string category)
        //{
        //    Customisable[] list = FindObjectsOfType<Customisable>();
        //    List<GameObject> gos = new List<GameObject>();
        //    foreach (Customisable c in list)
        //    {
        //        if (c.type == customisables)
        //            gos.Add(c.gameObject);
        //    }

        //    Selection.objects = gos.ToArray();
        //}

        public void OnValidate()
        {
            GetPackageVersion();
            RefreshCustomisableList();
            UpdateCustomisables();
        }

        public void Reset()
        {
            RefreshCustomisableList();
            UpdateCustomisables();
        }

        public void UpdateCustomisables()
        {
            if (!Customisable.customizer)
                Customisable.customizer = this;

            foreach (Customisable customisable in customisablesInScene)
            {
                if (!Customisable.customizer)
                    customisable.UpdateDescription();
            }
        }

        public void RefreshCustomisableList()
        {
            //Don't refresh the list in the prefab asset
            if (!PrefabUtility.IsPartOfPrefabAsset(this))
            {
                Customisable[] tempCustomisablesInScene = FindObjectsOfType<Customisable>();

                //If there are elements in it
                if (activeCustomisablesInScene.Any())
                {
                    //Only add the new ones at the end (Useful to maintain list order)
                    foreach (Customisable custo in tempCustomisablesInScene)
                    {
                        if (customisablesInScene.Contains(custo)) continue;
                        else customisablesInScene.Add(custo);

                        if (activeCustomisablesInScene.Contains(custo)) continue;
                        else
                        {
                            if (custo.isActiveAndEnabled)
                                activeCustomisablesInScene.Add(custo);
                        }
                    }
                }
                //if not start from scratch
                else
                {
                    foreach (Customisable custo in tempCustomisablesInScene)
                        if (custo.isActiveAndEnabled)
                            activeCustomisablesInScene.Add(custo);

                    customisablesInScene = tempCustomisablesInScene.ToList();

                    JLogBase.Log("Created new customisables lists", this);
                }
            }

            //Remove emptyentries
            activeCustomisablesInScene.RemoveAll(item => item == null);
            customisablesInScene = customisablesInScene.Distinct().ToList();
            customisablesInScene.RemoveAll(item => item == null);

            //activeCustomisablesInScene = activeCustomisablesInScene.Distinct().ToList();
            //customisablesInScene = customisablesInScene.Distinct().ToList();
        }

        public void SetCustomisablesOrder()
        {

        }
#endif

        #endregion

        #region Runtime logic

        public void Awake()
        {
            LogPackageVersion();
        }

        public void Update()
        {
            if (Debug.isDebugBuild && uncustomizedCustomisables != null)
            {
                if (Input.GetKeyDown(KeyCode.A))
                    ToggleUnCustomizedActivation();
            }
        }

        public void GetPackageVersion()
        {
#if UNITY_EDITOR
            foreach (PackageInfo packageInfo in PackageInfo.GetAllRegisteredPackages())
            {
                if (packageInfo.name == "live.rlty.scenetools")
                    sceneToolsVersion = packageInfo.version;
            }
#endif
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
                        JLogBase.LogError(
                            "Invalid key type in scene description: key=" + k.key + " value=" + k.value + " type=" +
                            entry.type, this);
                    else
                        SearchAndCustomize(type, fullList, k);
                }
            }

#if UNITY_EDITOR
            JLogBase.Log("\n <b>All gameobject that does not receive a value for customisation will be deactivated," +
                         "that can happen for two reasons: </b> \n" +
                         "1) It's SceneDescription key is not up to date and need to be regenerated <i>(Toolbar/RLTY/CreateSceneDescription)</i>\n" +
                         "2) No value as being assigned to its key, look for the corresponding <i>SessionConfig</i> ScriptableObject in the asset folder\n\n",
                this);
#else
            JLog("All gameobject that does not receive a value for customisation will be deactivated, it means that no value has been set in the Event Configuration tab of the event");
#endif

            CustomisationManagerHandlerData.CustomisationFinished();
            JLogBase.Log("Finished Customization", this);
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
                JLogWarning("No customisable found for key=" + k.key + " type=" + type);
        }

        private void DeactivateUnCustomized()
        {
            uncustomizedCustomisables = new List<Customisable>();

            foreach (Customisable customisable in customisablesInScene)
            {
                if (customisable.deactivable)
                {
                    if (customisable._keyValue == null || customisable._keyValue.value.IsNullOrWhitespace())
                    {
                        customisable.gameObject.SetActive(false);
                        uncustomizedCustomisables.Add(customisable);
                    }
                }
            }

            JLogBase.Log("Deactivated " + uncustomizedCustomisables.Count +
                         " unmodified customisables, press A in development build to toggle activation of those.",
                this);
        }

        private void ToggleUnCustomizedActivation()
        {
            foreach (Customisable custo in uncustomizedCustomisables)
            {
                if (custo.gameObject.activeInHierarchy)
                    custo.gameObject.SetActive(false);
                else
                    custo.gameObject.SetActive(true);
            }
        }

        public override void EventHandlerRegister()
        {
            AssetDownloaderManagerHandlerData.OnAllDownloadFinished += CustomizeScene;
            CustomisationManagerHandlerData.OnSceneCustomized += DeactivateUnCustomized;
        }

        public override void EventHandlerUnRegister()
        {
            AssetDownloaderManagerHandlerData.OnAllDownloadFinished -= CustomizeScene;
            CustomisationManagerHandlerData.OnSceneCustomized -= DeactivateUnCustomized;
        }

        #endregion
    }
}