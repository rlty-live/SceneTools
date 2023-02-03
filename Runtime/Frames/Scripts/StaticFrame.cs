using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StaticFrame : SceneTool
{
    public Vector2Int ScreenSize = new Vector2Int(1920, 1080);
    [PropertyRange(1,10)]
    public float Scale = 1;

    public string ID;
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
        
        if(String.IsNullOrEmpty(ID)) Gizmos.color = Color.red;
        
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f * Scale);
        
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = trs;
        
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));

        string idDisplay = ID;
        if (String.IsNullOrEmpty(idDisplay))
        {
            Handles.color = Color.red;
            idDisplay = "No ID";
        }
        
        Handles.Label(transform.position, "Frame : "+ idDisplay);
        
        color.a = 125;
        Gizmos.color = color;
        
        if(String.IsNullOrEmpty(ID)) Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));
    }
#endif
}
