using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using RLTY.Customisation;
using UnityEngine.InputSystem;

namespace Judiva.Metaverse.Interactions
{
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
            Handles.DrawWireDisc(transform.position, transform.up, spawnRadius);
#endif
        }

        //[Button("Stick to ground")]
        private void SlapOntoSurface()
        {
            
        }
    }
}
