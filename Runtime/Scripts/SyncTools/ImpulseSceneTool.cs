using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu("RLTY/SceneTools/Impulse")]
public class ImpulseSceneTool : SceneTool
{
    [Header("Impulse Data")] 
    public Transform Target;
    public float ImpulseMagnitude;
    public float Damages = 10;
    public Vector3 AxisFilter = Vector3.one;
    public Vector3 ColliderCenter = Vector3.zero;

    public bool BoxCollider;
    [ShowIf(nameof(BoxCollider))] public Vector3 BoxColliderSize = Vector3.zero;
    
    public bool SphereCollider;
    [ShowIf(nameof(SphereCollider))] public float SphereColliderRadius = 0;

    public bool CapsuleCollider;
    [ShowIf(nameof(CapsuleCollider))] public float CapsuleColliderRadius = 0;
    [ShowIf(nameof(CapsuleCollider))] public float CapsuleColliderHeight = 0;
    
    protected override bool IsDataValid()
    {
        return Target != null;
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.red;

        if (BoxCollider)
        {
            Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, Vector3.one);
            DrawCube(ColliderCenter, BoxColliderSize);
        }
        else if (SphereCollider)
        {
            Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, Vector3.one);
            DrawSphere(ColliderCenter, SphereColliderRadius);
        }
        else if (CapsuleCollider)
        {
            if (CapsuleColliderHeight <= 2 * CapsuleColliderRadius)
            {
                Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, Vector3.one);
                DrawSphere(ColliderCenter, CapsuleColliderRadius);
                return;
            }
            
            Vector3 scaleFactor = new Vector3(1, 0.1f, 1);
            Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, scaleFactor);
            DrawSphere(ColliderCenter/ scaleFactor.y, CapsuleColliderRadius);
            
            scaleFactor = new Vector3(0.3f, 0.05f, 0.3f);
            Gizmos.matrix = Matrix4x4.TRS(Target.position, Target.rotation, scaleFactor);
            Vector3 scaledCenter = Vector3.Scale(ColliderCenter, new Vector3(1/scaleFactor.x, 1/scaleFactor.y, 1/scaleFactor.z));
            Vector3 heightOffset = Vector3.up * CapsuleColliderHeight / scaleFactor.y / 2;
            DrawSphere(scaledCenter - heightOffset, CapsuleColliderRadius);
            DrawSphere(scaledCenter + heightOffset, CapsuleColliderRadius);
        }
    }

#if UNITY_EDITOR
    
    [Button]
    private void DetectColliderOnTarget()
    {
        if (!Target.TryGetComponent(out Collider coll))
        {
            Debug.LogWarning($"This target has no collider on it");
            BoxCollider = SphereCollider = CapsuleCollider = false;
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
                Vector3 scale = sphere.transform.lossyScale;
                float scaledRadius = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z)) * sphere.radius;
                SphereColliderRadius = scaledRadius;
                break;
            
            case CapsuleCollider capsule:
                CapsuleCollider = true;
                scale = capsule.transform.lossyScale;
                scaledRadius = Mathf.Max(scale.x, scale.z) * capsule.radius;
                CapsuleColliderRadius = scaledRadius;
                CapsuleColliderHeight = capsule.height * scale.y;
                break;
            
            default:
                Debug.LogWarning($"This target has an unsupported collider type ({coll.GetType().Name})");
                break;
        }
        
        UpdateData();
    }
    
    private bool _boxCollider;
    private bool _sphereCollider;
    private bool _capsuleCollider;
    
    private void OnValidate()
    {
        UpdateData();
    }
    
    private void UpdateData()
    {
        bool resetBox = false;
        bool resetSphere = false;
        bool resetCapsule = false;
        
        if (BoxCollider != _boxCollider)
        {
            resetSphere = resetCapsule = true;
            _boxCollider = BoxCollider;
        }

        if (SphereCollider != _sphereCollider)
        {
            resetBox = resetCapsule = true;
            _sphereCollider = SphereCollider;
        }

        if (CapsuleCollider != _capsuleCollider)
        {
            resetBox = resetSphere = true;
            _capsuleCollider = CapsuleCollider;
        }

        if (CapsuleCollider && CapsuleColliderHeight < CapsuleColliderRadius * 2)
        {
            CapsuleColliderHeight = CapsuleColliderRadius * 2;
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

        if (resetCapsule)
        {
            CapsuleCollider = _capsuleCollider = false;
            CapsuleColliderRadius = 0;
            CapsuleColliderHeight = 0;
        }
    }
    
#endif
}