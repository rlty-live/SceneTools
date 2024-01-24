using UnityEngine;

public abstract class ActionSceneTool : SceneTool
{
    [Header("Action Data")] 
    public Transform Target;
    public bool ExecuteAtStart = false;
    
    protected override bool IsDataValid()
    {
        return Target != null;
    }
}
