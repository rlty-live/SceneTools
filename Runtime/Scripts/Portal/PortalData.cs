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
            Gizmos.color = Color.cyan;

            Gizmos.DrawCube(transform.position + (Vector3.up * 0.75f), new Vector3(1.5f, 1.5f, 0.1f));
        }
#endif
    }
}