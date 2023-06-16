using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using RLTY.Customisation;
using UnityEngine.InputSystem;

namespace Judiva.Metaverse.Interactions
{
    [HideMonoScript]
    public class SpawnPoint : MonoBehaviour
    {
        public static float spawnRadius = 5;
        Color gizmoColor = Color.cyan;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 1);

            gizmoColor.a = 100;

#if UNITY_EDITOR
            Handles.color = gizmoColor;

            Vector3 lineStart = transform.position + transform.forward;

            Handles.DrawLine(lineStart, lineStart + transform.forward);
            Handles.DrawWireDisc(transform.position, transform.up, spawnRadius);
#endif
        }

        [Button("Stick to ground")]
        private void StickToGround()
        {
            transform.Rotate(Vector3.right, -90f);
            SlapOntoSurface.SlapThisOntoSurface(transform, 10);
            transform.Rotate(Vector3.right, +90f);
        }
    }
}