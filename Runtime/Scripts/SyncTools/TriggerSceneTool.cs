using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class TriggerSceneTool : SceneTool
{
    [Title("TriggerSceneTool")] 
    public List<ActionSceneTool> ActionToolsToTrigger;
    public bool TriggerOnlyOnce = false;
    public Vector3 TriggerSize = Vector3.one;
    
    protected override bool IsDataValid()
    {
        return ActionToolsToTrigger != null && ActionToolsToTrigger.Count > 0;
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, TriggerSize);
    }
}