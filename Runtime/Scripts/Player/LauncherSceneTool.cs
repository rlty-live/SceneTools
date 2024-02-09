using Sirenix.OdinInspector;
using UnityEngine;

public abstract class LauncherSceneTool : SceneTool
{
    [Header("Launcher Data")]
    [ShowIf(nameof(ShowGizmo))] public bool DrawTrajectoryOnlyOnSelected;
    [ShowIf(nameof(ShowGizmo))] public float LauncherGizmoRadius = 0.5f;
    
    protected abstract void DrawTrajectory(Color color);
}