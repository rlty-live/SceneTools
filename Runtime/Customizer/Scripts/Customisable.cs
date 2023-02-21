using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Customisation
{
#if UNITY_EDITOR
    [HideMonoScript]
    [CanEditMultipleObjects]
    [HelpURL("https://www.youtube.com/watch?v=xvFZjo5PgG0")]
    [AddComponentMenu("RLTY/Customisable/Customisable")]
    [DisallowMultipleComponent]
#endif
    public class Customisable : RLTYMonoBehaviour
    {
        #region Global variables
        [Title("Configuration")]
        public bool deactivable = true;

        [LabelWidth(40), Space(5)]
        public CustomisableType type;

        [HorizontalGroup("IDs"), LabelText("Key"), LabelWidth(40)]
        [SerializeField]
        public string displayKey;
        [ReadOnly, ShowIf("showUtilities", true), LabelText("realKey")]
        public string key;
        [SerializeField]
        [HorizontalGroup("IDs", 10, 0)]
        [ShowIf("showUtilities", true)]
        private bool useGameobjectName;

        [Title("Organization")]
        [SerializeField]
        private static List<string> sections = new List<string>();
        [SerializeField]
        private static List<string> groups = new List<string>();
        [SerializeField]
        private static List<string> labelGroups = new List<string>();

        const int labelWidth = 80;

        [ValueDropdown("GetSections")]
        [Tooltip("Customisables in the same section appear in a panel named 'Section'")]
        [LabelWidth(labelWidth)]
        public string section;
        [SerializeField, HorizontalGroup("NewSection"), ShowIf("showUtilities", true), HideLabel]
        private string newSection;
        [ValueDropdown("GetGroups")]
        [Tooltip("Grouped customisable appear in the same bloc, without labels")]
        [LabelWidth(labelWidth)]
        public string group;
        [SerializeField, HorizontalGroup("NewGroup"), ShowIf("showUtilities", true), HideLabel]
        private string newGroup;
        [ValueDropdown("GetLabelGroups")]
        [Tooltip("To be added")]
        [LabelWidth(labelWidth)]
        public string labelGroup;
        [SerializeField, HorizontalGroup("NewLabelGroup"), ShowIf("showUtilities", true), HideLabel]
        private string newLabelGroup;

        [Title("Description")]
        [SerializeField, ShowIf("showUtilities")]
        [Tooltip("Set this to true if you haven't already added your groupind and sectionning")]
        private bool manualDescription = true;
        [TextArea, LabelText("Commentary")]
        public string displayCommentary;
        [TextArea, ReadOnly, ShowIf("showUtilities", true), LabelText("Real Commentary")]
        public string commentary;

        [SerializeField, ReadOnly]
        string technicalInfo;

        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        private KeyValueBase _keyValue;

        [Title("Setup")]
        [ShowIf("showUtilities", true), ReadOnly]
        public Processor processor;

        [SerializeField] private Component target;

        private static CustomisationManager customizer;

        [Title("Handles")]
        [SerializeField]
        [ShowIf("showUtilities", true)]
        private bool showGizmo = false;
        [ShowIf("showUtilities", true)]
        public Vector3 gizmoOffset = new Vector3(1, 0, 0);
        #endregion

        #region EditorOnly Logic
#if UNITY_EDITOR
        public IEnumerable<string> GetGroups() { return groups; }
        public IEnumerable<string> GetSections() { return sections; }
        public IEnumerable<string> GetLabelGroups() { return labelGroups; }

        [Button, HorizontalGroup("NewGroup"), ShowIf("showUtilities", true)]
        public void AddGroup()
        {
            if (newGroup != null && newGroup != string.Empty)
                customizer.groups.Add(newGroup);
        }
        [Button, HorizontalGroup("NewSection"), ShowIf("showUtilities", true)]
        public void AddSection()
        {
            if (newGroup != null && newGroup != string.Empty)
                customizer.sections.Add(newSection);
        }
        [Button, HorizontalGroup("NewLabelGroup"), ShowIf("showUtilities", true)]
        public void AddLabelGroup()
        {
            if (newLabelGroup != null && newLabelGroup != string.Empty)
                customizer.groupLabel.Add(newSection);
        }

        public void UpdateCustomisableOrganisation()
        {
            if (groups.Count == 0)
                group = string.Empty;
            if (sections.Count == 0)
                section = string.Empty;
            if (labelGroups.Count == 0)
                labelGroup = string.Empty;

            if (!customizer)
                customizer = FindObjectOfType<CustomisationManager>();
            else
            {
                customizer.groupLabel = labelGroups;
                customizer.groups = groups;
                customizer.sections = sections;
            }
        }

        public void UpdateKey()
        {
            if (useGameobjectName)
                displayKey = transform.name;

            key = displayKey;

            if (!key.IsNullOrWhitespace())
                key = key.Replace(" ", "_");
        }
        public void UpdateCommentary()
        {
            if (!manualDescription && !displayCommentary.IsNullOrWhitespace())
                commentary = labelGroups.IndexOf(labelGroup) + " #" + labelGroup + "/" + section + "$" + group + "_" + displayCommentary;
        }


        public void GetTechnicalInfo()
        {
            //no processor-specific code here, please call a method on the processor instead
        }

        public void OnValidate()
        {
            UpdateKey();

            UpdateCustomisableOrganisation();

            if (!manualDescription)
                UpdateCommentary();

            CheckForProcessor();

            GetTechnicalInfo();
        }

        public void Reset() => useGameobjectName = true;

        public void OnDrawGizmos()
        {
            if (showGizmo)
            {
                Vector3 gizmoPosition = transform.position + gizmoOffset;
                Gizmos.DrawIcon(gizmoPosition, "Customisation Logo.png", true);

                if (gizmoOffset.magnitude > 1)
                    Gizmos.DrawLine(transform.position, gizmoPosition);
            }
        }
#endif
        #endregion

        #region Common Logic
        public override void CheckSetup()
        {
            base.CheckSetup();

            CheckForProcessor();
            //CheckForCustomisationManager();
        }

        /// <summary>
        /// Check this gameobject and his children for a processor and add a compatible one if possible
        /// </summary>
        [ExecuteInEditMode]
        public void CheckForProcessor()
        {
            if (TryGetComponent(out Processor _proc))
                processor = _proc;

            if (processor)
            {
                if (CustomisableUtility.Processors.ContainsKey(type) && CustomisableUtility.Processors[type].type == processor.GetType())
                    ValidProcessorDebugLog(true);
                else
                {
                    if (!Application.isPlaying)
                        DestroyProcessor(processor);
                    processor = null;
                }

            }

            if (processor == null && CustomisableUtility.Processors.ContainsKey(type))
            {
                processor = (Processor)gameObject.AddComponent(CustomisableUtility.Processors[type].type);
                ValidProcessorDebugLog(false);
            }

            if (processor)
            {
                target = processor.FindComponent();
                if (target == null)
                    JLogError("Processor target not found on " + name + " type=" + processor.GetType());
            }
            else
                JLogError("No processor found for type=" + type + " on " + name);
        }

        public void DestroyProcessor(Processor _processor)
        {
#if UNITY_EDITOR
            if (!PrefabUtility.IsPartOfPrefabAsset(this))
                DestroyImmediate(_processor);
#endif
        }

        [ExecuteInEditMode]
        private string ValidProcessorDebugLog(bool valid)
        {
            if (debug)
            {
                if (valid)
                {
#if UNITY_EDITOR
                    if (!PrefabUtility.IsPartOfPrefabAsset(this) && this.gameObject.activeInHierarchy)
                        StartCoroutine(TemporaryBoolSwitch(3));
#endif
                    return "Processor is present and compatible";
                }

                else
                {
                    JLogWarning("No Processor found in children, added " + processor + " automatically, please set it up.");
#if UNITY_EDITOR
                    if (!PrefabUtility.IsPartOfPrefabAsset(this) && this.gameObject.activeInHierarchy)
                        StartCoroutine(TemporaryBoolSwitch(3));
#endif
                    return "No Processor found in children, added one automatically, please set it up.";
                }
            }

            else
                return string.Empty;
        }

        #endregion

        #region Runtime Logic
        //Replace all Customize Method with a KeyValueBaseType Parameter and Make KeyValueObject Inherit from KeyValueBase
        public void Customize(KeyValueBase _KeyValueBase)
        {
            CheckSetup();
            _keyValue = _KeyValueBase;
            if (!processor)
            {
                JLog("Missing processor on Customisable please check setup");
            }

            else
            {
                if (target == null)
                {
                    JLogError("Target is null on customisable " + processor.name);
                    return;
                }
                JLog("Using processor " + processor.GetType());
                processor.Customize(_keyValue);
            }
        }

        public void DeactivateGameobjectIfIntact()
        {
            if (deactivable == true)
            {
                if (_keyValue == null || _keyValue.value.IsNullOrWhitespace())
                {
                    gameObject.SetActive(false);
                    JLog("No customisation asked for this customisable, deactivating Gameobject");
                }
            }
        }

        #region Observer Pattern

        public override void EventHandlerRegister()
        {
            CustomisationManagerHandlerData.OnSceneCustomized += DeactivateGameobjectIfIntact;
        }

        public override void EventHandlerUnRegister()
        {
            CustomisationManagerHandlerData.OnSceneCustomized -= DeactivateGameobjectIfIntact;
        }

        #endregion

        #endregion
    }
}



//Keep for after OdinRemoval
//Source: https://www.youtube.com/watch?v=ThcSHbVh7xc
//Allows to create dropdown Lists from Lists

//[System.Serializable]
//public class GroupEntry
//{
//    [ReadOnly]
//    public string groupName;
//    [ShowIf("renaming"), SerializeField]
//    private string newName;

//    public bool renaming;

//    [Button, HorizontalGroup("Edit")]
//    public void Remove()
//    {

//    }

//    [Button, HorizontalGroup("Edit")]
//    public void Rename() => renaming = true;

//    [Button, HorizontalGroup("Edit")]
//    public void Validate()
//    {
//        groupName = newName;
//        renaming = false;
//    }
//}

//public class ListToDropDownSelector : PropertyAttribute
//{
//    public Type myType;
//    public string propertyName;

//    public ListToDropDownSelector(Type _myType, string _propertyName)
//    {
//        myType = _myType;
//        propertyName = _propertyName;
//    }
//}

//[CustomPropertyDrawer(typeof(ListToDropDownSelector))]
//public class ListToDropDownSelectorDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        ListToDropDownSelector atb = attribute as ListToDropDownSelector;
//        List<string> stringList = null;

//        if (atb.myType.GetField(atb.propertyName) != null)
//            stringList = atb.myType.GetField(atb.propertyName).GetValue(atb.myType) as List<string>;

//        if (stringList != null && stringList.Count != 0)
//        {
//            int selectedIndex = Mathf.Max(stringList.IndexOf(property.stringValue), 0);

//            selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, stringList.ToArray());
//            property.stringValue = stringList[selectedIndex];
//            property.stringValue = stringList[selectedIndex];
//        }
//        else
//            EditorGUI.PropertyField(position, property, label);
//    }
//}
