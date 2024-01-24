using Sirenix.OdinInspector;
using UnityEngine;

public class ImpulseSceneTool : SceneTool
{
    [Header("Impulse Data")] 
    public Transform Target;
    public float ImpulseMagnitude;
    public Vector3 AxisFilter = Vector3.one;

    public bool BoxCollider;
    [ShowIf(nameof(BoxCollider))] public Vector3 BoxColliderSize = Vector3.zero;
    
    public bool SphereCollider;
    [ShowIf(nameof(SphereCollider))] public float SphereColliderRadius = 0;

    public bool CylinderCollider;
    [ShowIf(nameof(CylinderCollider))] public float CylinderColliderRadius = 0;
    [ShowIf(nameof(CylinderCollider))] public float CylinderColliderHeight = 0;
    
    protected override bool IsDataValid()
    {
        return Target != null;
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.red;

        if (BoxCollider) Gizmos.DrawCube(Target.position, BoxColliderSize);
        if (SphereCollider) Gizmos.DrawSphere(Target.position, SphereColliderRadius);
        if (CylinderCollider)
        {
            Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, new Vector3(1, 0.1f, 1));
            Vector3 heightVector = new Vector3(0, CylinderColliderHeight, 0);
            Gizmos.DrawSphere(-heightVector, CylinderColliderRadius);
            Gizmos.DrawSphere(Vector3.zero, CylinderColliderRadius);
            Gizmos.DrawSphere(heightVector, CylinderColliderRadius);
        }
    }

#if UNITY_EDITOR
    
    [Button]
    private void DetectColliderOnTarget()
    {
        if (!Target.TryGetComponent(out Collider coll))
        {
            Debug.LogWarning($"This target has no collider on it");
            return;
        }

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
            
            default:
                Debug.LogWarning($"This target has an unsupported collider type ({coll.GetType().Name})");
                break;
        }
        
        CheckDataValidity();
    }
    
    private void OnValidate()
    {
        CheckDataValidity();
    }
    
    private bool _boxCollider;
    private bool _sphereCollider;
    private bool _cylinderCollider;
    
    private void CheckDataValidity()
    {
        bool resetBox = false;
        bool resetSphere = false;
        bool resetCylinder = false;
        
        if (BoxCollider != _boxCollider)
        {
            resetSphere = resetCylinder = true;
            _boxCollider = BoxCollider;
        }

        if (SphereCollider != _sphereCollider)
        {
            resetBox = resetCylinder = true;
            _sphereCollider = SphereCollider;
        }

        if (CylinderCollider != _cylinderCollider)
        {
            resetBox = resetSphere = true;
            _cylinderCollider = CylinderCollider;
        }
        
        /////

        if (resetBox)
        {
            BoxCollider = _boxCollider = false;
            BoxColliderSize = Vector3.zero;
        }

        if (resetSphere)
        {
            SphereCollider = _sphereCollider = false;
            SphereColliderRadius = 0;
        }

        if (resetCylinder)
        {
            CylinderCollider = _cylinderCollider = false;
            CylinderColliderRadius = 0;
            CylinderColliderHeight = 0;
        }
    }
    
#endif
}