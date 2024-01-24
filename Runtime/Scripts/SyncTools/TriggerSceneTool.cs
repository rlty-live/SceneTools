using System.Collections.Generic;
using UnityEngine;

public class TriggerSceneTool : SceneTool
{
    [Header("Trigger Data")] 
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
        foreach (ActionSceneTool tool in ActionToolsToTrigger)
        {
            Gizmos.DrawLine(transform.position, tool.Target.position);
        }
    }
}