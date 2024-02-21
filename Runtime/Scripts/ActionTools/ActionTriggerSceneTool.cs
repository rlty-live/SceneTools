using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum TriggerMode
{
    OnlyActivate,
    OnlyReset,
    Both
}

[AddComponentMenu("RLTY/SceneTools/Action Trigger")]
public class ActionTriggerSceneTool : SceneTool
{
    [TitleGroup("Trigger Data")] 
    public bool DrawLinesTowardsTargets;
    [Space]
    public TriggerMode TriggerMode;
    public bool TriggerOnlyOnce = false;
    [Space]
    public List<ActionSceneTool> ActionToolsToTrigger;
    public bool FollowTarget = false;
    [ShowIf(nameof(FollowTarget))] public ActionSceneTool TargetToFollow;
    [Space]
    public Vector3 TriggerSize = Vector3.one;

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        DrawCube(Vector3.zero, TriggerSize);
        
        Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        foreach (ActionSceneTool tool in ActionToolsToTrigger)
        {
            if (tool == null || !DrawLinesTowardsTargets) continue;
            Gizmos.DrawLine(transform.position, tool.Target.position);
        }
    }

    private void OnValidate()
    {
        if (FollowTarget)
        {
            if(TargetToFollow == null) return;
            
            if (ActionToolsToTrigger != null && !ActionToolsToTrigger.Contains(TargetToFollow))
            {
                Debug.LogWarning($"{name} (Id = {Id}): the target to follow must be registered as an action to trigger, target added to the list.");
                ActionToolsToTrigger.Add(TargetToFollow);
            }
        }
        else
        {
            TargetToFollow = null;
        }
    }
}