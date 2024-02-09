using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Tooltip("Defines how the rotation is computed. \n\n" +
         "Fast: fastest way to reach value that never rotates beyond 360°. \n\n" +
         "FastBeyond360: fastest way to reach value that rotates beyond 360°. \n\n" +
         "WorldAxisAdd: adds the value to the transform using world axis. \n\n" +
         "LocalAxisAdd: adds the value  to the transform's local axis.")]
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
    [TitleGroup("Rotation Data")] 
    public Vector3 RotationValue = Vector3.zero;
    public ERotateMode RotateMode;
    
    protected override void DrawGizmos()
    {
#if UNITY_EDITOR
        
        if(RotationValue.x % 360 == 0 && RotationValue.y % 360 == 0 && RotationValue.z % 360 == 0) return;
        
        Gizmos.color = Color.yellow;
       
        if (!EditorApplication.isPlaying || (EditorApplication.isPlaying && _finalPositionMeshes.Count == 0))
        {
            if (RotateMode is ERotateMode.LocalAxisAdd or ERotateMode.WorldAxisAdd)
            {
                Target.localRotation *= Quaternion.Euler(RotationValue); 
                RefreshFinalPositionMeshes();
                Target.localRotation *= Quaternion.Euler(-RotationValue); 
            }
            else
            {
                Quaternion initRotation = Target.localRotation;
                Target.localRotation = Quaternion.Euler(RotationValue); 
                RefreshFinalPositionMeshes();
                Target.localRotation = initRotation; 
            }
        }
        else
        {
            foreach (var gizmoMesh in _finalPositionMeshes)
            {
                DrawMesh(gizmoMesh.Mesh, gizmoMesh.Position, gizmoMesh.Rotation, gizmoMesh.LossyScale);
            }
        }
        
#endif
    }
}