using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SocialWall : SceneTool
{
    [PropertyRange(1,10)]
    public float Scale = 1;


    private Vector2 _ScreenSize = new Vector2(2280, 1080);
    [HideInInspector] public Vector2 ScreenSize => _ScreenSize;
    
    
    
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector2 screenSize = _ScreenSize / 1080 * Scale;
        
        Color32 color = new Color(0.1f, 0, 0.5f);
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
