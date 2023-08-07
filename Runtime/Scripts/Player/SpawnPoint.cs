using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using RLTY.Customisation;
using UnityEngine.InputSystem;
using System;

namespace Judiva.Metaverse.Interactions
{
    [HideMonoScript]
    public class SpawnPoint : MonoBehaviour
    {
        public static float spawnRadius = 5;
        Color gizmoColor = Color.cyan;

        private void OnDrawGizmos()
        {
            float sphereRadius = 0.3f;

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, sphereRadius);

            gizmoColor.a = 100;

#if UNITY_EDITOR
            Handles.color = gizmoColor;

            Vector3 lineStart = transform.position + transform.forward * sphereRadius;

            Handles.DrawLine(lineStart, lineStart + transform.forward);
            Handles.DrawWireDisc(transform.position, transform.up, spawnRadius);
            Handles.Label(lineStart + transform.forward * 1.2f, "Front");
            Handles.Label(transform.position + transform.forward * spawnRadius, "Max spawn area");
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