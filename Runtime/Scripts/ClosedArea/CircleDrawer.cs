using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
#if UNITY_EDITOR
    public float multiplier = 1f;
    private void OnDrawGizmos()
    {
        Color32 color = Color.green;
        color.a = 255;
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawWireDisc(transform.position + Vector3.up * 0.3f, Vector3.up, transform.localScale.x * 1.5f * multiplier);
    }
#endif
}
