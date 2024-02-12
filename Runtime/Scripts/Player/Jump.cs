using UnityEngine;

namespace Judiva.Metaverse.Interactions
{
    public class Jump : LauncherSceneTool
    {
        [Header("Jump data")]
        public float VerticalVelocity = 5;
        [Range(-180, 180)]
        public float OrientationOffset = 0;
        public float AdditionalSpeed = 0;
        public Quaternion Orientation => Quaternion.Euler(new Vector3(0, OrientationOffset, 0)) * transform.rotation;
        
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
            
            Vector3 r = Orientation * Vector3.forward;

            //draw trajectory
            int count = 10000;

            //in order to test, we need to access speed and gravity
            float runSpeed = 5.33f;
            float walkSpeed = 2;
            float gravity = -15;
            Vector3 velocity = AdditionalSpeed * r + VerticalVelocity * Vector3.up;
            Vector3 pos = transform.position;
            Vector3 posMax = transform.position;
            float dt = 0.016f;
            for (int i = 0; i < count; i++)
            {
                Vector3 newPos = pos + (velocity+walkSpeed * r) * dt;
                velocity.y += gravity * dt;
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
                newPos = posMax + (velocity + runSpeed * r) * dt;
                Gizmos.DrawLine(posMax, newPos);
                posMax = newPos;

                if (pos.y < transform.position.y)
                    break;
            }
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

