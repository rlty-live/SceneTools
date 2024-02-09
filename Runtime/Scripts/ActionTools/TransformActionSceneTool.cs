using System.Collections.Generic;
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

[Tooltip("Defines if the transformation is a loop and how it should behave when completed. \n\n" +
         "None: the transformation is not looped and plays only once. If a reset method is selected, it is then applied. \n\n" +
         "Restart: the target snaps to its initial transform and the transformation starts again. \n\n" +
         "Yoyo (ping-pong): the Target plays the transformation in reverse. \n\n" +
         "Incremental: the Target plays the same transformation from its current transform.")]
public enum ELoopType
{
    None,
    Restart,
    Yoyo,
    Incremental,
}

[Tooltip("Defines if and how the Target should go back to its initial transform when the transformation is completed and not a loop. \n\n" +
         "None: nothing is done, the Target stays in place. \n\n" +
         "Rewind: the Target plays the transformation in reverse. \n\n" +
         "Fast: the Target takes the fastest way (straight line for position & smallest angle for rotation)\n\n" +
         "Snap: the Target snaps instantly.")]
public enum EResetMethod
{
    None,
    Rewind,
    Fast,
    Snap,
}

#endregion

public class TransformActionSceneTool : ActionSceneTool
{
    [TitleGroup("Transformation Base Data")]
    public float Duration = 1f;

    public bool IsEased => EaseType != EaseType.None;
    [InlineButton(nameof(OpenEaseHelper), "   Open Ease Helper   ")]
    public EaseType EaseType = EaseType.None;
    [ShowIf(nameof(EaseType), EaseType.In)] public EaseInFunction EaseInFunction = EaseInFunction.InSine;
    [ShowIf(nameof(EaseType), EaseType.Out)] public EaseOutFunction EaseOutFunction = EaseOutFunction.OutSine;
    [ShowIf(nameof(EaseType), EaseType.InOut)] public EaseInOutFunction EaseInOutFunction = EaseInOutFunction.InOutSine;
    [ShowIf(nameof(EaseType), EaseType.CustomCurve)] public AnimationCurve CustomCurve = null;
    
    public bool IsLoop => LoopType != ELoopType.None;
    public ELoopType LoopType = ELoopType.None;
    
    public bool ResetOnComplete => ResetMethod != EResetMethod.None;
    [HideIf(nameof(IsLoop))] public EResetMethod ResetMethod = EResetMethod.None;
    [ShowIf(nameof(ResetOnComplete))] public float DelayToPerformReset = 0f;
    [ShowIf("@ResetOnComplete && ResetMethod != EResetMethod.Snap")] public float ResetDuration;
    
    private void OpenEaseHelper()
    {
        Application.OpenURL("https://easings.net/");
    }
    
#if UNITY_EDITOR

    private void OnValidate()
    {
        if (IsLoop) ResetMethod = EResetMethod.None;
    }
    
    public struct GizmoMesh
    {
        public Mesh Mesh;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LossyScale;
    }

    protected List<GizmoMesh> _finalPositionMeshes = new List<GizmoMesh>();
    
    protected void RefreshFinalPositionMeshes()
    {
        _finalPositionMeshes.Clear();
        
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                GizmoMesh gizmoMesh = new GizmoMesh()
                {
                    Mesh = meshFilter.sharedMesh,
                    Position = childTr.position,
                    Rotation = childTr.rotation,
                    LossyScale = childTr.lossyScale
                };
                _finalPositionMeshes.Add(gizmoMesh);
                
                DrawMesh(meshFilter.sharedMesh, childTr.position, childTr.rotation, childTr.lossyScale);
            }
        }
    }

#endif
    
}