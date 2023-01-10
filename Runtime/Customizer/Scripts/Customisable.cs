using System.Collections.Generic;
using UnityEngine;
//using VLB;
using Sirenix.OdinInspector;

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
        public bool IsDeactivable = false;
        [LabelWidth(40)]
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

        //Wip grouping of customisables
        public string group;
        static List<string> groupsList;

        [TextArea]
        public string commentary;
        [SerializeField, ReadOnly]
        string technicalInfo;

        private KeyValueBase _keyValue;

        [Title("Setup")]
        [ShowIf("showUtilities", true), ReadOnly]
        [InfoBox(infoBoxProcessor, InfoMessageType.None, "@this.type == CustomisableType.Video")]
        public Processor processor;
        
        [SerializeField] private Component target;

        [Title("Handles")]
        [SerializeField]
        [ShowIf("showUtilities", true)]
        private bool showGizmo = false;
        [ShowIf("showUtilities", true)]
        public Vector3 gizmoOffset = new Vector3(1, 0, 0);

        [HideInInspector]
        [SerializeField]
        private int _instanceId;

        const string infoBoxProcessor =
            "For a live stream to work, the configuration file key has to contain 'LiveStream'";

        #endregion

        #region EditorOnly Logic
#if UNITY_EDITOR

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
        public void UpdateKey()
        {
            if (useGameobjectName)
                displayKey = transform.name;
            key = displayKey;
            key = key.Replace(" ", "_");
        }

        public void GetTechnicalInfo()
        {
            //no processor-specific code here, please call a method on the processor instead
        }

        public void OnValidate()
        {
            UpdateKey();
            CheckForProcessor();
            GetTechnicalInfo();
        }

        public void Reset()
        {
            useGameobjectName = true;
        }

        //WIP automatic listing of existing groups and addition to an enum
        //Should allow to remove a group (and update corresponding Customisables)
        //Should warn if group already exists
        //Should allow to sort Customisable in a editor window according to group / Type or both
        public void UpdateEnum()
        {

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
        //PUT THE DESTROY IN A COROUTINE TO ENSURE ADDING NEWONE AFTER AND BEING ABLE TO YIELD
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
                    DestroyProcessor(processor);
                    processor = null;
                }

            }
            if (processor == null && CustomisableUtility.Processors.ContainsKey(type))
            {
                processor = (Processor)gameObject.AddComponent(CustomisableUtility.Processors[type].type);
                ValidProcessorDebugLog(false);
            }
            target = processor.FindComponent();
            if (target == null)
                Debug.LogError("Processor target not found on " + name+" type="+processor.GetType());
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
                    Debug.LogWarning("No Processor found in children, added " + processor + " automatically, please set it up.", this);
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

        public void ReturnToInitialState()
        {

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
                if (debug)
                    Debug.Log("Missing processor on Customisable please check setup", this);
            }

            else
            {
                if (target == null)
                {
                    Debug.LogError("Target is null on customisable " + processor.name);
                    return;
                }
                processor.Customize(_keyValue);
            }
        }

        #region Observer Pattern

        public override void EventHandlerRegister()
        {
        }

        public override void EventHandlerUnRegister()
        {
        }

        #endregion

        #endregion
    }
}

[System.Serializable]
public class GroupEntry
{
    [ReadOnly]
    public string groupName;
    [ShowIf("renaming"), SerializeField]
    private string newName;

    public bool renaming;

    [Button, HorizontalGroup("Edit")]
    public void Remove()
    {

    }

    [Button, HorizontalGroup("Edit")]
    public void Rename() => renaming = true;

    [Button, HorizontalGroup("Edit")]
    public void Validate()
    {
        groupName = newName;
        renaming = false;
    }
}