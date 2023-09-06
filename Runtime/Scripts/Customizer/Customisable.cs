using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Serialization;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Customisation
{
    [HideMonoScript]
    [HelpURL("https://www.youtube.com/watch?v=xvFZjo5PgG0")]
    [AddComponentMenu("RLTY/Customisable/Customisable")]
    [DisallowMultipleComponent]
    public class Customisable : RLTYMonoBehaviourBase
    {
        #region Global variables

        [Title("Configuration")]
        //[InfoBox("If you want to copy this gameobject just give it the same name and it will be customized the same way")]
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
        private static List<string> sections = new List<string>();
        private static List<string> groups = new List<string>();
        private static List<string> labelGroups = new List<string>();

        [ValueDropdown("GetSections")]
        [Tooltip("Customisables in the same section appear in a panel named 'Section'")]
        [ShowIf("customizer")]
        [LabelWidth(labelWidth)]
        public string section;

        [ValueDropdown("GetGroups")]
        [Tooltip("Grouped customisable appear in the same bloc, without labels")]
        [ShowIf("customizer")]
        [LabelWidth(labelWidth)]
        public string group;

        [ValueDropdown("GetLabelGroups")]
        [Tooltip("Customisables grouped in LabelGroups appear under a label parent to sections")]
        [ShowIf("customizer")]
        [LabelWidth(labelWidth)]
        public string labelGroup;

        [Title("Description")]
        [TextArea, FormerlySerializedAs("displayCommentary")]
        public string label;
        [TextArea, ReadOnly, ShowIf("showUtilities", true), FormerlySerializedAs("commentary")]
        public string _description;

        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        public KeyValueBase _keyValue;

        [Title("Setup")]
        [ShowIf("showUtilities", true), ReadOnly]
        public Processor processor;
        [SerializeField] private Component target;

        [Title("Handles")]
        [SerializeField]
        [ShowIf("showUtilities", true)]
        private bool showGizmo = false;

        [ShowIf("showUtilities", true)]
        public Vector3 gizmoOffset = new Vector3(1, 0, 0);

        public static CustomisationManager customizer;
        const int labelWidth = 80;
        string technicalInfo;
        #endregion

        #region EditorOnly Logic

#if UNITY_EDITOR

        public IEnumerable<string> GetGroups()
        {
            List<string> category = new List<string>();

            if (customizer)
                category = customizer.groups;
            else
                category = FindOrCreateCustomisationManager().groups;

            return category;
        }

        public IEnumerable<string> GetSections()
        {
            List<string> category = new List<string>();

            if (customizer)
                category = customizer.sections;
            else
                category = FindOrCreateCustomisationManager().sections;

            return category;
        }

        public IEnumerable<string> GetLabelGroups()
        {
            List<string> category = new List<string>();

            if (customizer)
                category = customizer.groupLabels;
            else
                category = FindOrCreateCustomisationManager().groupLabels;

            return category;
        }

        public void UpdateKey()
        {
            if (useGameobjectName)
                displayKey = transform.name;

            key = displayKey;

            if (!key.IsNullOrWhitespace())
                key = key.Replace(" ", "_");
        }

        public void UpdateDescription()
        {
            if (!label.IsNullOrWhitespace())
            {
                string indexStr = string.Empty;
                string labelGroupStr = string.Empty;
                string sectionStr = string.Empty;
                string groupStr = string.Empty;

                if (customizer != null)
                {
                    indexStr = customizer.activeCustomisablesInScene.IndexOf(this).ToString() + " #";
                }

                labelGroupStr = string.IsNullOrWhiteSpace(labelGroup) ? string.Empty : labelGroup + "/";
                sectionStr = string.IsNullOrWhiteSpace(section) ? string.Empty : section + "$";
                groupStr = string.IsNullOrWhiteSpace(group) ? string.Empty : group + "_";

                _description = indexStr + labelGroupStr + sectionStr + groupStr + label;
            }
        }

        public void GetTechnicalInfo()
        {
            //no processor-specific code here, please call a method on the processor instead
        }

        public CustomisationManager FindOrCreateCustomisationManager()
        {
            if (!FindObjectOfType<CustomisationManager>())
            {
                GameObject customisationManager;
                customisationManager = new GameObject("Customisation Manager", typeof(CustomisationManager));
                customizer = customisationManager.GetComponent<CustomisationManager>();

                JLogBase.Log("No CustomisationManager present in the scene, added one.", customizer);
                //Selection.activeObject = customizer;

                return customizer;
            }
            else
            {
                return FindObjectOfType<CustomisationManager>();
            }
        }

        public void OnValidate()
        {
            UpdateKey();
            CheckForProcessor();
            GetTechnicalInfo();
            UpdateDescription();
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

        public void OnGUI()
        {
            FindOrCreateCustomisationManager();
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
                if (CustomisableUtility.Processors.ContainsKey(type) &&
                    CustomisableUtility.Processors[type].type == processor.GetType())
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
                    if (!PrefabUtility.IsPartOfPrefabAsset(this) && gameObject.activeInHierarchy)
                        StartCoroutine(TemporaryBoolSwitch(3));
#endif
                    return "Processor is present and compatible";
                }

                else
                {
                    JLogBase.LogWarning("No Processor found in children, added " + processor + " automatically, please set it up.", this);
#if UNITY_EDITOR
                    if (!PrefabUtility.IsPartOfPrefabAsset(this) && gameObject.activeInHierarchy)
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
                JLogBase.Log("Missing processor on Customisable please check setup", this);
            }

            else
            {
                if (target == null)
                {
                    JLogBase.LogError("Target is null on customisable " + processor.name, this);
                    return;
                }

                //JLogBase.Log("Using processor " + processor.GetType(), this);
                processor.Customize(_keyValue);
            }
        }

        #region Observer Pattern

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