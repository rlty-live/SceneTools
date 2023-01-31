using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using RLTY.UI;

[HideMonoScript]
[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(RLTYMouseEvent))]
public class Zoomable : SceneTool
{
    public const string warningMessage = "For now a zoomable must have specifically a BoxCollider component.";

    [ReadOnly, InfoBox(warningMessage)]
    public Collider col;

    public void OnValidate()
    {
        if (TryGetComponent(out BoxCollider _col)) col = _col;
        else Debug.Log(warningMessage);
    }
}
