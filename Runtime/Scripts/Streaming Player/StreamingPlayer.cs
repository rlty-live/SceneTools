using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StreamingPlayer : SceneTool
{
    [TitleGroup("Settings")]
    public string ID;
    public string Commentary;
    public bool MutePlayersWhenPlyingVideo = false;
    [FormerlySerializedAs("TargetMeshes")] public List<MeshRenderer> TargetMeshesRenderer;


    [TitleGroup("Size")]
    [ReadOnly]
    public Vector2Int ScreenSize = new Vector2Int(1920, 1080);
    [PropertyRange(1,50)]
    public float Scale = 1;
    // [PropertyRange(1,10)] private float Width = 1.77777777f;
    // [PropertyRange(1,10)] private float Height = 9;

    [HideInInspector][ReadOnly]
    public int BaselineSize = 1080;
    
    
    [TitleGroup("Trigger")]
    public enum ColliderTypeEnum
    {
        Global,
        Sphere,
        Box,
        Mesh
    }
    
    public ColliderTypeEnum ColliderType = ColliderTypeEnum.Box;
    
    [ShowIf(nameof(ColliderType), ColliderTypeEnum.Sphere)]
    [PropertyRange(1,200)]
    public float SphereColliderDiameter = 5;

    [ShowIf(nameof(ColliderType), ColliderTypeEnum.Box)]
    public Vector3 BoxColliderSize = new Vector3(3, 2, 5);
    
    [HideIf(nameof(ColliderType), ColliderTypeEnum.Mesh)]
    [HideIf(nameof(ColliderType), ColliderTypeEnum.Global)]
    public Vector3 ColliderOffset = new Vector3(0, 0, 0);

    [ShowIf(nameof(ColliderType), ColliderTypeEnum.Mesh)][Required("You need to specify a mesh to define the collider bounds. If not, this audio zone will be ignored")]
    public Mesh ColliderMesh;
    
    
    
    

#if UNITY_EDITOR
    private void Reset()
    {
        ID = SceneManager.GetActiveScene().name + "_Frame_" + DateTime.Now.Second + DateTime.Now.Millisecond;
        OnValidate();
    }

    private void OnValidate()
    {
        ID = ID.Replace(" ", "_");
        SetSize();
    }

    
    
    private void SetSize()
    {
        ScreenSize = new Vector2Int(1920, 1080);
    }
    
    private Color32 _FrameBaseColor = new Color(0.5f, 0.5f, 1);

    private Color32 GetGizmoColor()
    {
        Color32 color = Color.white;
        return color;
    }

    void OnDrawGizmos()
    {
        Vector2 screenSize = new Vector2(ScreenSize.x, ScreenSize.y) * Scale / 1080;

        Color32 gizmoColor = GetGizmoColor();
        gizmoColor.a = 255;
        Gizmos.color = gizmoColor;
        
        if(String.IsNullOrEmpty(ID)) Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + transform.up * Scale * 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f * Scale);
        
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = trs;
        
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));

        string idDisplay = ID;
        if (String.IsNullOrEmpty(idDisplay))
        {
            Handles.color = Color.red;
            idDisplay = "No ID";
        }

        Handles.Label(transform.position + transform.up * Scale * 0.6f, "Up");
        Handles.Label(transform.position + transform.forward * 0.5f * Scale, "Front");
        Handles.Label(transform.position + -transform.right * 0.5f * Scale, idDisplay);
        
        gizmoColor = _FrameBaseColor;
        gizmoColor.a = 125;
        Gizmos.color = gizmoColor;
        
        if(String.IsNullOrEmpty(ID)) Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(screenSize.x, screenSize.y, 0.0001f));
        
        
        
        
        //Trigger collider
        Matrix4x4 matricCollider = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = matricCollider;
        
        Color32 color = Color.green;
        color.a = 255;
        Color32 colorSeeThrough = color;
        colorSeeThrough.a = 64;
        
        Gizmos.color = color;
        Vector3 colliderGizmoPosition = Vector3.zero;
        if (ColliderType != ColliderTypeEnum.Mesh) colliderGizmoPosition += ColliderOffset;
        
        if (ColliderType == ColliderTypeEnum.Sphere)
        {
            Gizmos.DrawWireSphere(colliderGizmoPosition, SphereColliderDiameter/2);
            Gizmos.color = colorSeeThrough;
            Gizmos.DrawSphere(colliderGizmoPosition, SphereColliderDiameter/2);
        }

        if (ColliderType == ColliderTypeEnum.Box)
        {
            Gizmos.DrawWireCube(colliderGizmoPosition, BoxColliderSize);
            Gizmos.color = colorSeeThrough;
            Gizmos.DrawCube(colliderGizmoPosition, BoxColliderSize);
        }

        if (ColliderType == ColliderTypeEnum.Mesh && ColliderMesh)
        {
            Gizmos.DrawWireMesh(ColliderMesh,colliderGizmoPosition);
            Gizmos.color = colorSeeThrough;
            Gizmos.DrawMesh(ColliderMesh,colliderGizmoPosition);
        }
    }


    private void OnDrawGizmosSelected()
    {
        List<Transform> framesWithSameID = GeteveryFrameWithSameID();

        foreach (var otherFrameTransform in framesWithSameID)
        {
            Debug.DrawLine(otherFrameTransform.position, gameObject.transform.position, GetGizmoColor());
        }
    }
    
    
    
    
    private List<Transform> GeteveryFrameWithSameID()
    {
        StaticFrame[] AllStaticFrames = FindObjectsOfType<StaticFrame>();
        List<Transform> transforms= new List<Transform>();

        foreach (var staticFrame in AllStaticFrames)
        {
            if(staticFrame.ID == ID && !String.IsNullOrEmpty(ID)) transforms.Add(staticFrame.transform);
        }

        return transforms;
    }
    
    
    
    

    [TitleGroup("Positioning")]
    [PropertyRange(1,100)]
    public float OffsetFromSurfaceInMillimeters = 1;
    
    [Button]
    private void SlapOntoSurfaceBehind()
    {
        SlapOntoSurface.SlapThisOntoSurface(transform, OffsetFromSurfaceInMillimeters);
    }
    
#endif
}
