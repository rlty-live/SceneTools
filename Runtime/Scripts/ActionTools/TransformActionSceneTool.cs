using Sirenix.OdinInspector;
using UnityEngine;

#region Easing Enums

public enum EaseType
{
    None,
    In,
    Out,
    InOut,
    CustomCurve
}

public enum EaseInFunction
{
    InSine,
    InQuad,
    InCubic,
    InQuart,
    InQuint,
    InExpo,
    InCirc,
    InElastic,
    InBack,
    InBounce,
    InFlash,
}

public enum EaseOutFunction
{
    OutSine,
    OutQuad,
    OutCubic,
    OutQuart,
    OutQuint,
    OutExpo,
    OutCirc,
    OutElastic,
    OutBack,
    OutBounce,
    OutFlash,
}

public enum EaseInOutFunction
{
    InOutSine,
    InOutQuad,
    InOutCubic,
    InOutQuart,
    InOutQuint,
    InOutExpo,
    InOutCirc,
    InOutElastic,
    InOutBack,
    InOutBounce,
    InOutFlash,
}

public enum ELoopType
{
    Yoyo = 1,
    Incremental = 2,
} 

#endregion

public class TransformActionSceneTool : ActionSceneTool
{
    [Header("Transformation Data")]
    public Vector3 QuantityToAdd = Vector3.zero;
    public float Duration = 1f;
    [Space]
    public EaseType EaseType = EaseType.None;
    [ShowIf(nameof(EaseType), EaseType.In)] public EaseInFunction EaseInFunction = EaseInFunction.InSine;
    [ShowIf(nameof(EaseType), EaseType.Out)] public EaseOutFunction EaseOutFunction = EaseOutFunction.OutSine;
    [ShowIf(nameof(EaseType), EaseType.InOut)] public EaseInOutFunction EaseInOutFunction = EaseInOutFunction.InOutSine;
    [ShowIf(nameof(EaseType), EaseType.CustomCurve)] public AnimationCurve CustomCurve = null;
    [Space]
    public bool IsLoop = true;
    [ShowIf(nameof(IsLoop))] public ELoopType LoopType;
    [Space]
    [HideIf(nameof(IsLoop))]public bool ResetOnComplete = false;
    [ShowIf(nameof(ResetOnComplete))] public float DelayToPerformReset = 0f;
    
#if UNITY_EDITOR

    private void OnValidate()
    {
        if (IsLoop) ResetOnComplete = false;
    }
    
    [PropertySpace(15)]
    [Button]
    private void OpenEaseFunctionsHelper()
    {
        Application.OpenURL("https://easings.net/");
    }

#endif
    
}