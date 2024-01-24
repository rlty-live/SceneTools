using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class SceneTool : RLTYMonoBehaviourBase
{
    [Header("Tool Data")] 
    public bool ShowGizmo = true;
    
    [ReadOnly] public int Id;
    
    protected virtual bool IsDataValid()
    {
        return true;
    }
    
    protected virtual void DrawGizmos(){}
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying && ShowGizmo && IsDataValid()) 
            DrawGizmos();
    }
#endif
    
}