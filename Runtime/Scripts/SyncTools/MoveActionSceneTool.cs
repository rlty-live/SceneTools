using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MoveActionSceneTool : ActionSceneTool
{
    [Title("MoveActionSceneTool")]
    public Transform Target = null;
    public Vector3 MoveToAdd = Vector3.zero;
    public float Duration = 1f;
    public Ease EaseFunction = Ease.Linear;
    public bool IsLoop = true;
    [HideIf(nameof(IsLoop))]public bool ResetOnComplete = false;

    protected override bool IsDataValid()
    {
        return Target != null;
    }
    
    protected override void DrawGizmos()
    {
        if(MoveToAdd == Vector3.zero) return;
        
        Gizmos.color = Color.yellow;

        Vector3 initialPosition = Target.position;
        Quaternion initialRotation = Target.rotation;
        Vector3 initialScale = Target.localScale;
        
        Target.Translate(MoveToAdd, Space.World);
        
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                Gizmos.DrawMesh(meshFilter.sharedMesh, childTr.position, childTr.rotation, childTr.lossyScale);
            }
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(initialPosition, Target.position);
        
        Target.SetPositionAndRotation(initialPosition, initialRotation);
        Target.localScale = initialScale;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (IsLoop) ResetOnComplete = false;
    }

#endif
    
}