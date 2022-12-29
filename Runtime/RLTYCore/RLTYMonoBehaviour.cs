using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class RLTYMonoBehaviour : RLTYMonoBehaviourBase
{
    public abstract void EventHandlerRegister();
    public abstract void EventHandlerUnRegister();

    public virtual void Start() => EventHandlerRegister();
    public virtual void OnDestroy() => EventHandlerUnRegister();
}

public abstract class RLTYMonoBehaviourBase : JMonoBehaviour
{
    [Space(10), Title("Utilities")]
    [SerializeField, PropertyOrder(30), LabelWidth(90)]
    protected bool showUtilities;

    [Button, PropertyOrder(31)] 
    [HorizontalGroup("debug", MinWidth = 100, MaxWidth = 100)]
    public virtual void CheckSetup() {}

    [SerializeField, ShowIf("showUtilities", true), ReadOnly]
    [PropertyOrder(32), HorizontalGroup("debug", LabelWidth = 90)]
    protected bool correctSetup;

    [SerializeField, ShowIf("showUtilities", true), PropertyOrder(33)]
    [HorizontalGroup("debug", VisibleIf = "showUtilities", LabelWidth = 20)]
    protected bool debug = true;

    protected bool slowTrigger;

    #region ToolBox
    [ExecuteAlways]
    public virtual IEnumerator TemporaryBoolSwitch(int duration)
    {
        slowTrigger = true;
        yield return new WaitForSeconds(duration);

        slowTrigger = false;
        yield return null;
    }
    [ExecuteAlways]
    public static void DestroyEditorSafe(Component component)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            Destroy(component);
        else
            DestroyImmediate(component);
#else
                Destroy(component);
#endif
    }
    [ExecuteAlways]
    public static void DestroyEditorSafe(GameObject gameObject)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
#else
                Destroy(gameObject);
#endif
    }
    #endregion
}