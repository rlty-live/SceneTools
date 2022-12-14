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

        [SerializeField, ShowIf("showUtilities", true), ReadOnly, Space(5)]
        private Customisable[] loadedCustomisables;

        [SerializeField, DetailedInfoBox("How to use the Customisable Lists", howTo), Space(10)]
        private List<CustomisableList> customisableLists;

        [SerializeField, ShowIf("showUtilities")]
        private CustomisableSelector[] tempLoadedCustomisableSelectors;

        [SerializeField] private List<CustomisableSelector> customisablesSelectors;

        [PropertyOrder(40)] [SerializeField, HorizontalGroup("selector", Title = "Tools"), LabelWidth(100)]
        private CustomisableType customisables;

        //[SerializeField]
        //private List<CustomisableSharedMaterial> CustomisableSharedMaterials;

        private CustomisableDeactivator deactivator = null;

        private const string howTo =
            "To easily navigate through the list add a second inspector side-by-side and lock this one. " +
            "Then navigate in the list by double clicking any Customisable List";
        #endregion

        #region EditorOnly logic
#if UNITY_EDITOR
        public void OnValidate()
        {
            GetCustomisables();
        }

        /// <summary>
        /// Select all Customisable corresponding to CustomisablesToSelect in Hierarchy
        /// </summary>
        [Button("Select"), HorizontalGroup("selector")]
        public void SelectAll()
        {
            foreach (CustomisableList list in customisableLists)
            {
                if (list.type == customisables)
                {
                    Customisable[] customisables = list.customisablesDictionnary.Values.ToArray<Customisable>();
                    List<GameObject> gos = new List<GameObject>();
                    GameObject[] gOsToSelect;

                    foreach (Customisable custo in customisables)
                        gos.Add(custo.gameObject);

                    gOsToSelect = gos.ToArray();
                    Selection.objects = gOsToSelect;
                }
            }
        }

        public List<CustomisableList> GetCustomisableLists()
        {
            return customisableLists;
        }

        public Customisable[] GetLoadedCustomisables()
        {
            return loadedCustomisables;
        }

        public List<CustomisableSelector> GetCustomisableSelectors()
        {
            return customisablesSelectors;
        }
#endif
        #endregion

        #region Runtime logic

        /// <summary>
        /// Sort all loaded customisable into lists sorted by Customisable Type
        /// </summary>
        [Button("Refresh lists"), ShowIf("showUtilities"), HorizontalGroup("Buttons"), PropertyOrder(20)]
        void GetCustomisables()
        {
            loadedCustomisables = FindObjectsOfType<Customisable>();

            customisableLists = new List<CustomisableList>();

            //Allow for the enum to change length, so we can add other types in the future
            for (int i = 0; i < System.Enum.GetValues(typeof(CustomisableType)).Length; i++)
            {
                customisableLists.Add(ScriptableObject.CreateInstance<CustomisableList>());
                customisableLists[customisableLists.Count - 1].type = (CustomisableType)i;
                customisableLists[customisableLists.Count - 1].name = customisableLists[customisableLists.Count - 1].type.ToString();
            }

            //((CustomisableType)i))

            //Check loadedScenes for customisable and create typed lists
            foreach (Customisable customisable in loadedCustomisables)
            {
                for (int i = 0; i < customisableLists.Count; i++)
                {
                    if (customisable.type == customisableLists[i].type)
                    {
                        if (!customisableLists[i].customisablesDictionnary.ContainsKey(customisable.key))
                            customisableLists[i].customisablesDictionnary.Add(customisable.key, customisable);

                        else
                        {
                            if (debug)
                                Debug.Log(customisable + " customisable key: " + customisable.key + ", is a duplicate, please rename it");
                        }
                    }
                }
            }

            if (debug) Debug.Log("Customisable List has been refreshed", this);
        }

        void GetCustomisablesSelector()
        {
            tempLoadedCustomisableSelectors = FindObjectsOfType<CustomisableSelector>();

            customisablesSelectors = new List<CustomisableSelector>();

            foreach (CustomisableSelector customisableSelector in tempLoadedCustomisableSelectors)
            {
                customisablesSelectors.Add(customisableSelector);
            }
        }

        /// <summary>
        /// Call to get a list of loaded Customsiable of type _customisableType
        /// </summary>
        /// <param name="_customisableType">The type of customsiable you want to get back</param>
        /// <returns></returns>
        CustomisableList GetCustomisableList(CustomisableType _customisableType)
        {
            foreach (CustomisableList customisableList in customisableLists)
            {
                if (customisableList.type == _customisableType)
                {
                    return customisableList;
                }
            }

            return null;
        }

        /// <summary>
        /// Asks every loaded and activated customisable to modify its correponding Components
        /// </summary>
        /// <param name="processedSceneDescription">The configuration file that lists all customisation keys and values for this building and this event</param>
        public void CustomizeScene(ProcessedSceneDescription processedSceneDescription)
        {
            if (debug) Debug.Log("Starting Customization from " + processedSceneDescription, this);

            if (!deactivator)
            {
                deactivator = TryGetComponent(out CustomisableDeactivator customisableDeactivator) ? customisableDeactivator : gameObject.AddComponent<CustomisableDeactivator>();
            }

            deactivator.SetCustomisables(loadedCustomisables.ToList());
            deactivator.SetDeactivateKeys(processedSceneDescription.sceneDescription.deactivatorKey);
            // deactivator.RemoveNotDeactivable();
            deactivator.DeactivateCustomisable();

            CustomisableList currentCustomisableList;

            CustomisationManagerHandlerData.CustomizationStarted();
            bool foundMatchingCustomisable = false;

            foreach (KeyValueObject kVO in processedSceneDescription.downloadedObjects)
            {
                foundMatchingCustomisable = false;

                if (kVO.GetType() == typeof(KeyValueTexture))
                {
                    if (debug) Debug.Log("Searching scene for Customisable matching " + kVO.key, this);

                    //Get the corresponding customisableList
                    currentCustomisableList = GetCustomisableList(CustomisableType.Texture);

                    //If the keys match, start customizing
                    if (currentCustomisableList != null && currentCustomisableList.customisablesDictionnary != null)
                        foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                            if (customisable.key.Contains(kVO.key))
                            {
                                customisable.Customize(kVO);
                                foundMatchingCustomisable = true;

                                if (debug)
                                    Debug.Log("Matched " + kVO.key + " with " + customisable, this);
                            }

                    currentCustomisableList = GetCustomisableList(CustomisableType.Sprite);
                    if (currentCustomisableList != null && currentCustomisableList.customisablesDictionnary != null)
                        foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                            if (customisable.key.Contains(kVO.key))
                            {
                                customisable.Customize(kVO);
                                foundMatchingCustomisable = true;

                                if (debug)
                                    Debug.Log("Matched " + kVO.key + " with " + customisable, this);
                            }
                }

                    if (!foundMatchingCustomisable)
                        if (debug)
                            Debug.Log("Could find any Customisable with a key containing: " + kVO.key);

                //if (kVO.GetType() == typeof(KeyValueAudio))
                //{
                //
                //}
                //
                //IF KVO == model (downloaded from separate asset bundle, ex: welcome robot)
            }

            foreach (KeyValueBase keyText in processedSceneDescription.sceneDescription.text)
            {
                foundMatchingCustomisable = false;

                if (debug) Debug.Log("Searching scene for Customisable matching " + keyText.key, this);

                currentCustomisableList = GetCustomisableList(CustomisableType.Text);

                //If the keys match, start customizing
                foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                {
                    if (customisable.key.Contains(keyText.key))
                    {
                        customisable.Customize(keyText);
                        foundMatchingCustomisable = true;

                        if (debug)
                            Debug.Log("Matched " + keyText.key + " with " + customisable, this);
                    }

                    if (!foundMatchingCustomisable)
                        if (debug)
                            Debug.Log("Could find any Customisable with a key containing: " + keyText.key);
                }
            }

            foreach (ColorKeyValue keyColor in processedSceneDescription.sceneDescription.color)
            {
                foundMatchingCustomisable = false;
                if (debug) Debug.Log("Searching scene for Customisable matching " + keyColor.key, this);

                currentCustomisableList = GetCustomisableList(CustomisableType.Color);

                //If the keys match, start customizing
                foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                {
                    if (customisable.key.Contains(keyColor.key))
                    {
                        customisable.Customize(keyColor);
                        foundMatchingCustomisable = true;

                        if (debug)
                            Debug.Log("Matched " + keyColor.key + " with " + customisable, this);
                    }
                }

                if (!foundMatchingCustomisable)
                    if (debug)
                        Debug.Log("Could find any Customisable with a key containing: " + keyColor.key);
            }

            //For Video Stream, UrlKeyValue's key will be parsed to check for "LiveStream" in the key
            foreach (UrlKeyValue keyVideoStream in processedSceneDescription.sceneDescription.videoFeed)
            {
                foundMatchingCustomisable = false;
                if (debug) Debug.Log("Searching scene for Customisable matching " + keyVideoStream.key, this);

                currentCustomisableList = GetCustomisableList(CustomisableType.Video);

                //WIP Solution, we should be able to separates booleans for specific purposes as this one
                if (keyVideoStream.key.Contains("LiveStream"))
                {
                    string tempKey = keyVideoStream.key.Replace("LiveStream", "");

                    foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                    {
                        //WIP Solution, we should be able to separates booleans for specific purposes as this one
                        if (customisable.key.Contains(tempKey))
                        {
                            customisable.Customize(keyVideoStream);
                            foundMatchingCustomisable = true;

                            if (debug)
                                Debug.Log("Matched " + keyVideoStream.key + " with " + customisable, this);
                        }
                    }
                }

                else
                {
                    foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                    {
                        //WIP Solution, we should be able to separates booleans for specific purposes as this one
                        if (customisable.key.Contains(keyVideoStream.key))
                        {
                            customisable.Customize(keyVideoStream);
                            foundMatchingCustomisable = true;

                            if (debug)
                                Debug.Log("Matched " + keyVideoStream.key + " with " + customisable, this);
                        }
                    }
                }

                if (!foundMatchingCustomisable)
                    if (debug)
                        Debug.Log("Could find any Customisable with a key containing: " + keyVideoStream.key);
            }

            //foreach (UrlKeyValue keyAudioStream in processedSceneDescription.sceneDescription.audioFeeds)
            //{
            //}

            foreach (UrlKeyValue keyExternalPage in processedSceneDescription.sceneDescription.externalPageUrl)
            {
                foundMatchingCustomisable = false;
                if (debug) Debug.Log("Searching scene for Customisable matching " + keyExternalPage.key, this);

                currentCustomisableList = GetCustomisableList(CustomisableType.ExternalPage);
                foreach (Customisable customisable in currentCustomisableList.customisablesDictionnary.Values)
                {
                    if (customisable.key.Contains(keyExternalPage.key))
                    {
                        customisable.Customize(keyExternalPage);

                        foundMatchingCustomisable = true;
                        if (debug)
                            Debug.Log("Matched " + keyExternalPage.key + " with " + customisable, this);
                }
            }

                if (!foundMatchingCustomisable)
                    if (debug) Debug.Log("Could find any Customisable with a key containing: " + keyExternalPage.key);
            }




            List<CustomisableSelector> tmpCustomisablesSelectors = new List<CustomisableSelector>();

            if (tmpCustomisablesSelectors.Equals(null)) return;

            foreach (CustomisableSelector customisableSelector in customisablesSelectors)
            {
                tmpCustomisablesSelectors.Add(customisableSelector);
            }

            foreach (KeyValueBase keyText in processedSceneDescription.sceneDescription.text)
            {
                foreach (CustomisableSelector customisableSelector in tmpCustomisablesSelectors)
                {
                    if (keyText.key.Contains(customisableSelector.CustomisableKey))
                    {
                        customisableSelector.Customize(keyText);
                        // tmpCustomisablesSelectors.Remove(customisableSelector);
                    }
                }
            }

            foreach (UrlKeyValue urlKeyValue in processedSceneDescription.sceneDescription.image)
            {
                foreach (CustomisableSelector customisablesSelector in tmpCustomisablesSelectors)
                {
                    if (urlKeyValue.key.Contains(customisablesSelector.CustomisableKey))
                    {
                        customisablesSelector.Customize(urlKeyValue);
                        // tmpCustomisablesSelectors.Remove(customisablesSelector);
                    }
                }
            }

            foreach (UrlKeyValue urlKeyValue in processedSceneDescription.sceneDescription.videoFeed)
            {
                foreach (CustomisableSelector customisableSelector in tmpCustomisablesSelectors)
                {
                    if (urlKeyValue.key.Contains(customisableSelector.CustomisableKey))
                    {
                        customisableSelector.Customize(urlKeyValue);
                        // tmpCustomisablesSelectors.Remove(customisableSelector);
                    }
                }
            }

            foreach (UrlKeyValue urlKeyValue in processedSceneDescription.sceneDescription.audioFeeds)
            {
                foreach (CustomisableSelector customisableSelector in tmpCustomisablesSelectors)
                {
                    if (urlKeyValue.key.Contains(customisableSelector.CustomisableKey))
                    {
                        customisableSelector.Customize(urlKeyValue);
                        // tmpCustomisablesSelectors.Remove(customisableSelector);
                    }
                }
            }

            foreach (UrlKeyValue urlKeyValue in processedSceneDescription.sceneDescription.externalPageUrl)
            {
                foreach (CustomisableSelector customisableSelector in tmpCustomisablesSelectors)
                {
                    if (urlKeyValue.key.Contains(customisableSelector.CustomisableKey))
                    {
                        customisableSelector.Customize(urlKeyValue);
                        // tmpCustomisablesSelectors.Remove(customisableSelector);
                    }
                }
            }

            if (debug) Debug.Log("Finished Customization from " + processedSceneDescription, this);
            CustomisationManagerHandlerData.CustomisationFinished();
        }

        /// <summary>
        /// WIP should permit to restore the scene and assets to their original states
        /// </summary>
        public void ResetScene()
        {
            foreach (CustomisableList customisables in customisableLists)
            foreach (Customisable customisable in customisables.customisablesDictionnary.Values)
                customisable.ReturnToInitialState();
        }

        #endregion

        #region Unity Callbacks

        public override void Start()
        {
            base.Start();
            GetCustomisables();
            GetCustomisablesSelector();
        }

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

    //[System.Serializable]
    //public class CustomisableSharedMaterial
    //{
    //    [HideInInspector]
    //    private string name;
    //    [SerializeField]
    //    private Material mat;
    //    [SerializeField]
    //    private CustomisableType type;
    //}

    #endregion
}