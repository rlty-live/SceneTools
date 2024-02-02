using UnityEngine;

[AddComponentMenu("RLTY/SceneTools/Move Action")]
public class MoveActionSceneTool : TransformActionSceneTool
{
    protected override void DrawGizmos()
    {
        if(QuantityToAdd == Vector3.zero) return;
        
        Gizmos.color = Color.yellow;

        Vector3 initialPosition = Target.position;
        Quaternion initialRotation = Target.rotation;
        Vector3 initialScale = Target.localScale;
        
        Target.Translate(QuantityToAdd, Space.World);
        
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                DrawMesh(meshFilter.sharedMesh, childTr.position, childTr.rotation, childTr.lossyScale);
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