using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StaticFrame : SceneTool
{
    [Title("Settings")]
    public string ID;
    public enum StaticFrameTypeEnum
    {
        StaticFramePublic,
        StaticFrameReservedToAdmins,
    }
    public StaticFrameTypeEnum Type = StaticFrameTypeEnum.StaticFramePublic;
    
    [Title("Size")]
    public Vector2Int ScreenSize = new Vector2Int(1920, 1080);
    [PropertyRange(1,10)]
    public float Scale = 1;

#if UNITY_EDITOR
    private void Reset()
    {
        ID = SceneManager.GetActiveScene().name + "_Frame_" + DateTime.Now.Second + DateTime.Now.Millisecond;
        OnValidate();
    }

    private void OnValidate()
    {
        ID = ID.Replace(" ", "_");
    }


    [Button]
    private void FlipXY()
    {
        Vector2Int newVector2 = new Vector2Int(ScreenSize.y, ScreenSize.x);
        ScreenSize = newVector2;
    }
    
    private Color32 FrameBaseColor = Color.blue;
    private Color32 AdminColor = new Color(0.1f, 0.5f, 1);

    void OnDrawGizmos()
    {
        Vector2 screenSize = new Vector2(ScreenSize.x, ScreenSize.y) / ScreenSize.y * Scale;
        Color32 color = Color.white;
        switch (Type)
        {
            case StaticFrameTypeEnum.StaticFramePublic:
                color = FrameBaseColor;
                break;
            case StaticFrameTypeEnum.StaticFrameReservedToAdmins:
                color = AdminColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
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
        
        color = FrameBaseColor;
        color.a = 125;
        Gizmos.color = color;
        
        if(String.IsNullOrEmpty(ID)) Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));
    }
#endif
}
