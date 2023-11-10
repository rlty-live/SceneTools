using UnityEditor;
using UnityEngine;

namespace Portal
{
    public class PortalData : NetworkSceneTool
    {
        public string URL;
        public bool StartActive;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero + Vector3.up * 0.75f, new Vector3(2.5f, 3.5f, 0.2f));

            Gizmos.matrix = previousMatrix;
        }
#endif
    }
}