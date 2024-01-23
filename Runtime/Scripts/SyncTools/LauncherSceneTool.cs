using Sirenix.OdinInspector;
using UnityEngine;

public abstract class LauncherSceneTool : SceneTool
{
    [Title("LauncherSceneTool")]
    public bool DrawTrajectoryOnlyOnSelected;
    public float LauncherGizmoRadius = 0.5f;
    
    protected abstract void DrawTrajectory(Color color);
}