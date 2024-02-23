using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("RLTY/SceneTools/Translate Action")]
public class TranslateActionSceneTool : TransformActionSceneTool
{
    [TitleGroup("Translation Data")] 
    public Vector3 TranslationValue = Vector3.zero;
    
    
    private Vector3 _finalTargetPos;
    
    protected override void DrawGizmos()
    {
#if UNITY_EDITOR
        
        if(TranslationValue == Vector3.zero) return;

        Gizmos.color = Color.yellow;
        
        if (!EditorApplication.isPlaying || (EditorApplication.isPlaying && _finalPositionMeshes.Count == 0))
        {
            Target.localPosition += TranslationValue;
            RefreshFinalPositionMeshes();
            _finalTargetPos = Target.position;
            Target.localPosition -= TranslationValue;
        }
        else
        {
            foreach (var gizmoMesh in _finalPositionMeshes)
            {
                DrawMesh(gizmoMesh.Mesh, gizmoMesh.Position, gizmoMesh.Rotation, gizmoMesh.LossyScale);
            }
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Target.position, _finalTargetPos);
        
#endif
    }
}