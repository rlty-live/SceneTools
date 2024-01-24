using UnityEngine;

namespace Judiva.Metaverse.Interactions
{
    public class Bumper : LauncherSceneTool
    {
        [Header("Bumper data")]
        public Transform Target;
        public float BumpHeight;
        public float TravelSpeed;

        [HideInInspector]
        public Vector3 StartPoint => transform.position;
        [HideInInspector]
        public Vector3 MidPoint => StartPoint + (Target.position - transform.position) / 2f + Vector3.up * BumpHeight;
        [HideInInspector]
        public Vector3 EndPoint => Target.position;
        

        protected override void DrawGizmos()
        {
            Gizmos.color = new Color(1.0f, 0.64f, 0.0f);
            Matrix4x4 DefaultMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 0.1f, 1));
            Gizmos.DrawSphere(Vector3.zero, LauncherGizmoRadius);
            Gizmos.DrawWireSphere(new Vector3(0,0.1f,0),LauncherGizmoRadius * 1.0f);
            Gizmos.DrawWireSphere(new Vector3(0,0.1f,0),LauncherGizmoRadius * 1.2f);
            Gizmos.matrix = DefaultMatrix;
            
            if (DrawTrajectoryOnlyOnSelected) return;
            DrawTrajectory(Gizmos.color);
        }

        protected override void DrawTrajectory(Color color)
        {
            Gizmos.color = color;
            
            Vector3 pos = StartPoint;
            
            int count = 10000;
            for (int i = 0; i < count; i++)
            {
                float progress = i / 10000f;
                Vector3 nextPos = CalculateArcPosition(StartPoint, MidPoint, EndPoint, progress);
                Gizmos.DrawLine(pos, nextPos);
                pos = nextPos;
            }
        }
        
        private Vector3 CalculateArcPosition(Vector3 startPoint, Vector3 middlePoint, Vector3 endPoint, float t)
        {
            float sineT = Mathf.Sin(t * Mathf.PI / 2); 
            float cosineT = 1 - Mathf.Cos(t * Mathf.PI / 2); 

            Vector3 lerpedPoint1 = Vector3.Lerp(startPoint, middlePoint, sineT);
            Vector3 lerpedPoint2 = Vector3.Lerp(middlePoint, endPoint, cosineT);
            
            return Vector3.Lerp(lerpedPoint1, lerpedPoint2, t);
        }
        
#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmo) return;
            DrawTrajectory(Color.blue);
        }
        
#endif
    }
}

