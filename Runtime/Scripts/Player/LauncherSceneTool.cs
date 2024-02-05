using UnityEngine;

public abstract class LauncherSceneTool : SceneTool
{
    [Header("Launcher Data")]
    public bool DrawTrajectoryOnlyOnSelected;
    public float LauncherGizmoRadius = 0.5f;
    
    protected abstract void DrawTrajectory(Color color);
}