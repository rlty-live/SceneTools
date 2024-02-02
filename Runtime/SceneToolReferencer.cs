using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("RLTY/SceneTools/SceneToolReferencer")]
public class SceneToolReferencer : SceneTool
{
    [Header("Referencer Data")] 
    public List<SceneTool> ToolsList = new List<SceneTool>();
    
    #if UNITY_EDITOR
    
    [Button]
    private void UpdateToolsList()
    {
        if (EditorApplication.isPlaying) return;
        
        ToolsList.Clear();
        SceneTool[] tools = FindObjectsOfType<SceneTool>();
        
        for (int i = 0; i < tools.Length; i++)
        {
            SceneTool tool = tools[i];
            if(tool.gameObject.scene.handle != gameObject.scene.handle) continue;
            if(tool == this) continue;

            tool.Id = ToolsList.Count;
            ToolsList.Add(tool);
            EditorUtility.SetDirty(tool);
        }
    }
    
#endif  
    
}