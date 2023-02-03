using UnityEngine;
using Sirenix.OdinInspector;
using RLTY.UI;

[HideMonoScript]
[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(RLTYMouseEvent))]
public class Zoomable : SceneTool
{
    [ReadOnly, InfoBox(warningMessage)]
    public Collider col;

    private bool showUtilities;

    public const string warningMessage = "For now a zoomable must have specifically a BoxCollider component. \n\n" +
    "The camera will move toward and face the object looking at it's bounds center, aligned with the positive Z axis (the blue line)";

#if UNITY_EDITOR
    [Title("Display")]
    private bool alwaysDisplay;
    [ShowIf("showUtilities"), SerializeField]
    private float cameraGizmoRadius = 0.3f;
    [ShowIf("showUtilities"), SerializeField]
    private float cameraLineDistance = 1, cameraLineLength = 2;


    public void OnDrawGizmos()
    {
        if (alwaysDisplay) DrawCameraGizmo();
    }

    public void OnDrawGizmosSelected() => DrawCameraGizmo();

    public void Reset() => showUtilities = true;

    public void OnValidate()
    {
        if (TryGetComponent(out BoxCollider _col)) col = _col;
        else Debug.Log(warningMessage);
    }

    //Possible upgrade: Add an Arrow model, or just a cone
    public void DrawCameraGizmo()
    {
        Vector3 cameraLineStart = new Vector3(0, 0, cameraLineDistance);
        Vector3 cameraLineEnd = new Vector3(0, 0, cameraLineDistance + cameraLineLength);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(col.bounds.center + cameraLineStart, col.bounds.center + cameraLineEnd - new Vector3(0, 0, cameraGizmoRadius * 2));
        Gizmos.DrawWireSphere(col.bounds.center + cameraLineEnd, cameraGizmoRadius);

        Gizmos.DrawLine(col.bounds.center + cameraLineEnd + new Vector3(cameraGizmoRadius, 0, 0), col.bounds.center + cameraLineEnd - new Vector3(0, 0, cameraGizmoRadius * 2));
        Gizmos.DrawLine(col.bounds.center + cameraLineEnd - new Vector3(cameraGizmoRadius, 0, 0), col.bounds.center + cameraLineEnd - new Vector3(0, 0, cameraGizmoRadius * 2));

        Gizmos.DrawLine(col.bounds.center + cameraLineEnd + new Vector3(0, cameraGizmoRadius, 0), col.bounds.center + cameraLineEnd - new Vector3(0, 0, cameraGizmoRadius * 2));
        Gizmos.DrawLine(col.bounds.center + cameraLineEnd - new Vector3(0, cameraGizmoRadius, 0), col.bounds.center + cameraLineEnd - new Vector3(0, 0, cameraGizmoRadius * 2));
    }
#endif
}
