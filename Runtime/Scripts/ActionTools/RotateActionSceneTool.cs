using UnityEngine;

public enum ERotateMode
{
    Fast,
    FastBeyond360,
    WorldAxisAdd,
    LocalAxisAdd,
}

[AddComponentMenu("RLTY/SceneTools/Rotate Action")]
public class RotateActionSceneTool : TransformActionSceneTool
{
    [Header("Rotation Data")] 
    public ERotateMode RotateMode;
    
    protected override void DrawGizmos()
    {
        if(QuantityToAdd.x % 360 == 0 && QuantityToAdd.y % 360 == 0 && QuantityToAdd.z % 360 == 0) return;
        
        Gizmos.color = Color.yellow;
       
        Vector3 initialPosition = Target.position;
        Quaternion initialRotation = Target.rotation;
        Vector3 initialScale = Target.localScale;
        
        Target.Rotate(QuantityToAdd);
        
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                DrawMesh(meshFilter.sharedMesh, childTr.position, childTr.rotation, childTr.lossyScale);
            }
        }
        
        Target.SetPositionAndRotation(initialPosition, initialRotation);
        Target.localScale = initialScale;
    }
}