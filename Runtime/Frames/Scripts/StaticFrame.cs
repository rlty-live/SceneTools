using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class StaticFrame : SceneTool
{
    public Vector2Int ScreenSize = new Vector2Int(1920, 1080);
    [PropertyRange(1,10)]
    public float Scale = 1;

    public string MediaUrl;


    [Button]
    private void FlipXY()
    {
        Vector2Int newVector2 = new Vector2Int(ScreenSize.y, ScreenSize.x);
        ScreenSize = newVector2;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector2 screenSize = new Vector2(ScreenSize.x, ScreenSize.y) / ScreenSize.y * Scale;
        
        Color32 color = Color.blue;
        color.a = 255;
        Gizmos.color = color;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f * Scale);
        
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = trs;
        
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));
        
        color.a = 125;
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));
    }
#endif
}
