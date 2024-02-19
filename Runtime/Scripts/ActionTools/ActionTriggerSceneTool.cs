using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu("RLTY/SceneTools/Action Trigger")]
public class ActionTriggerSceneTool : SceneTool
{
    [TitleGroup("Trigger Data")] 
    public List<ActionSceneTool> ActionToolsToTrigger;
    public bool OnlyOneTarget => ActionToolsToTrigger != null && ActionToolsToTrigger.Count == 1;
    [ShowIf(nameof(OnlyOneTarget))] public bool FollowTarget = false;
    
    public bool TriggerOnlyOnce = false;
    public Vector3 TriggerSize = Vector3.one;
        
    protected override bool IsDataValid()
    {
        return ActionToolsToTrigger != null && ActionToolsToTrigger.Count > 0;
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        DrawCube(Vector3.one, TriggerSize);
        foreach (ActionSceneTool tool in ActionToolsToTrigger)
        {
            if (tool == null) continue;
            Gizmos.DrawLine(transform.position, tool.Target.position);
        }
    }

    private void OnValidate()
    {
        if (!OnlyOneTarget)
        {
            FollowTarget = false;
        }
    }
}