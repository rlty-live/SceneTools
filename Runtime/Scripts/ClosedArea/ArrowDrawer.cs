using UnityEngine;

public class ArrowDrawer : MonoBehaviour
{
    public Vector3 dir;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Matrix4x4 PreviousMatrix = Gizmos.matrix;
        
        Vector3 finalDir = dir + transform.localRotation.eulerAngles;
        Quaternion rotation = Quaternion.Euler(transform.TransformDirection(finalDir));
        
        
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, rotation, Vector3.one);
        Gizmos.matrix = trs;
        
        Color32 color = Color.green;
        color.a = 255;
        
        Gizmos.color = color;
        DrawArrow.ForGizmo(Vector3.zero, Vector3.forward*2, Color.green, 0.25f, 30f);
        Gizmos.matrix = PreviousMatrix;
    }
#endif
}