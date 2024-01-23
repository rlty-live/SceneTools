using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class ImpulseSceneTool : SceneTool
{
    [Title("ImpulseSceneTool")] 
    public Transform Target;
    public float ImpulseMagnitude;
    public Vector3 AxisFilter = Vector3.one;

    public bool BoxCollider;
    [ShowIf(nameof(BoxCollider))] public Vector3 BoxColliderSize = Vector3.zero;
    
    public bool SphereCollider;
    [ShowIf(nameof(SphereCollider))] public float SphereColliderRadius = 0;
    
    protected override bool IsDataValid()
    {
        return Target != null;
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if(BoxCollider) Gizmos.DrawCube(Target.position, BoxColliderSize);
        if(SphereCollider) Gizmos.DrawSphere(Target.position, SphereColliderRadius);
    }

#if UNITY_EDITOR
    
    private bool _boxCollider;
    private bool _sphereCollider;
    
    private void OnValidate()
    {
        CheckDataValidity();
    }
    
    private void CheckDataValidity()
    {
        if (BoxCollider != _boxCollider)
        {
            SphereCollider = _sphereCollider = false;
            SphereColliderRadius = -1f;
            _boxCollider = BoxCollider;
        }

        if (SphereCollider != _sphereCollider)
        {
            BoxCollider = _boxCollider = false;
            BoxColliderSize = Vector3.zero;
            _sphereCollider = SphereCollider;
        }
    }
    
    [Button]
    private void DetectColliderOnTarget()
    {
        if (!Target.TryGetComponent(out Collider coll)) return;

        switch (coll)
        {
            case BoxCollider box:
                BoxCollider = true;
                BoxColliderSize = Vector3.Scale(box.transform.lossyScale, box.size);
                break;
            
            case SphereCollider sphere:
                SphereCollider = true;
                Vector3 sphereWorldScale = sphere.transform.lossyScale;
                float xyMax = Mathf.Max(Mathf.Abs(sphereWorldScale.x), Mathf.Abs(sphereWorldScale.y));
                float maxScale = Mathf.Max(xyMax, Mathf.Abs(sphereWorldScale.z));
                float scaledRadius = Mathf.Abs(maxScale * sphere.radius);
                SphereColliderRadius = scaledRadius;
                break;
        }
        
        CheckDataValidity();
    }
    
#endif
}