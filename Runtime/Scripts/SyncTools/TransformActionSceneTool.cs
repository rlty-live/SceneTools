using Sirenix.OdinInspector;
using UnityEngine;

public enum EEaseType
{
    Unset,
    Linear,
    InSine,
    OutSine,
    InOutSine,
    InQuad,
    OutQuad,
    InOutQuad,
    InCubic,
    OutCubic,
    InOutCubic,
    InQuart,
    OutQuart,
    InOutQuart,
    InQuint,
    OutQuint,
    InOutQuint,
    InExpo,
    OutExpo,
    InOutExpo,
    InCirc,
    OutCirc,
    InOutCirc,
    InElastic,
    OutElastic,
    InOutElastic,
    InBack,
    OutBack,
    InOutBack,
    InBounce,
    OutBounce,
    InOutBounce,
    Flash,
    InFlash,
    OutFlash,
    InOutFlash,
}

public enum ELoopType
{
    Yoyo,
    Incremental,
} 

public class TransformActionSceneTool : ActionSceneTool
{
    [Header("Transformation Data")]
    public Vector3 QuantityToAdd = Vector3.zero;
    public float Duration = 1f;
    public EEaseType EaseFunction = EEaseType.Linear;
    
    public bool IsLoop = true;
    [ShowIf(nameof(IsLoop))] public ELoopType LoopType;
    
    [HideIf(nameof(IsLoop))]public bool ResetOnComplete = false;
    [ShowIf(nameof(ResetOnComplete))] public float DelayToPerformReset = 0f;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (IsLoop) ResetOnComplete = false;
    }

#endif
    
}