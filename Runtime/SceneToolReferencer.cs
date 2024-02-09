using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("RLTY/SceneTools/SceneToolReferencer")]
public class SceneToolReferencer : SceneTool
{
    [TitleGroup("Referencer Data")] 
    public List<SceneTool> ToolsList = new List<SceneTool>();

#if UNITY_EDITOR

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private int _previousTotalToolsFound = 0;
    
    private void OnHierarchyChanged()
    {
        int newTotal = FindObjectsOfType<SceneTool>().Length; 
        if(newTotal != _previousTotalToolsFound)
        {
            UpdateToolsList();
            _previousTotalToolsFound = newTotal;
        }
    }

    private void Reset()
    {
        Id = -1;
    }

    [Button]
    private void UpdateToolsList()
    {
        if (EditorApplication.isPlaying) return;
        Id = -1;
        ToolsList.Clear();
        SceneTool[] tools = FindObjectsOfType<SceneTool>();
        foreach (SceneTool tool in tools)
        {
            if(tool.gameObject.scene.handle != gameObject.scene.handle) continue;
            if(tool == this) continue;

            tool.Id = ToolsList.Count;
            ToolsList.Add(tool);
            EditorUtility.SetDirty(tool);
        }
        Debug.Log("SceneTools list updated");
    }
    
#endif  
    
}