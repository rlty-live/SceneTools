using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class RotateActionSceneTool : ActionSceneTool
{
    [Title("RotationActionSceneTool")] 
    public Transform Target;
    public Vector3 RotationToAdd = Vector3.zero;
    public float Duration = 1f;
    public Ease EaseFunction = Ease.Linear;
    public bool IsLoop = true;

    protected override bool IsDataValid()
    {
        return Target != null;
    }
    
    protected override void DrawGizmos()
    {
        if(RotationToAdd.x % 360 == 0 && RotationToAdd.y % 360 == 0 && RotationToAdd.z % 360 == 0) return;
        
        Gizmos.color = Color.yellow;
       
        Vector3 initialPosition = Target.position;
        Quaternion initialRotation = Target.rotation;
        Vector3 initialScale = Target.localScale;
        
        Target.Rotate(RotationToAdd);
        
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                Gizmos.DrawMesh(meshFilter.sharedMesh, childTr.position, childTr.rotation, childTr.lossyScale);
            }
        }
        
        Target.SetPositionAndRotation(initialPosition, initialRotation);
        Target.localScale = initialScale;
    }
}