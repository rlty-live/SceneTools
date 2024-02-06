using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class SceneTool : RLTYMonoBehaviourBase
{
    [Header("Tool Data")]
    [HideIf("@Id < 0"), ReadOnly] public int Id = 0;
    [HideIf("@Id < 0")] public bool ShowGizmo = true, WiredGizmos = false; 
    // -1 is reserved for the SceneToolReferencer, which doesn't need those properties
    
    protected virtual bool IsDataValid()
    {
        return true;
    }
    
    protected virtual void DrawGizmos(){}

    protected void DrawCube(Vector3 center, Vector3 size)
    {
        if (WiredGizmos) Gizmos.DrawWireCube(center, size);
        else Gizmos.DrawCube(center, size);
    }
    
    protected void DrawSphere(Vector3 center, float radius)
    {
        if (WiredGizmos) Gizmos.DrawWireSphere(center, radius);
        else Gizmos.DrawSphere(center, radius);
    }
    
    protected void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (WiredGizmos) Gizmos.DrawWireMesh(mesh, position, rotation, scale);
        else Gizmos.DrawMesh(mesh, position, rotation, scale);
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying && ShowGizmo && IsDataValid()) 
            DrawGizmos();
    }

#endif
    
}