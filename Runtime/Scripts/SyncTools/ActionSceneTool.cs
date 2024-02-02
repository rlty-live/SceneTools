using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ActionSceneTool : SceneTool
{
    [Header("Action Data")] 
    public Transform Target;
    public bool ExecuteAtStart = false;
    public bool StartWithRandomDelay = false;
    [ShowIf(nameof(StartWithRandomDelay))] public Vector2 DelayRange;
    
    protected override bool IsDataValid()
    {
        return Target != null;
    }
}
