using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Judiva.Metaverse.Interactions
{

    [RequireComponent(typeof(TriggerZone))]
    public class TriggerJump : MonoBehaviour
    {
        public float verticalVelocity = 5;
        [Range(-180, 180)]
        public float orientationOffset = 0;
        public float additionalSpeed = 0;

        Quaternion Orientation { get { return Quaternion.Euler(new Vector3(0, orientationOffset, 0)) * transform.rotation; } }

        private void Start()
        {
            GetComponent<TriggerZone>().onPlayerEnter += (x) =>
            {
                AllPlayers.Me.SetAdditionalSpeed(additionalSpeed * (Orientation * Vector3.forward));
                AllPlayers.Me.SetVerticalVelocity(verticalVelocity);  
            };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Vector3 r = Orientation * Vector3.forward;

            //draw trajectory
            int count = 10000;

            //in order to test, we need to access speed and gravity
            float runSpeed = 5.33f;
            float walkSpeed = 2;
            float gravity = -15;
            Vector3 velocity = additionalSpeed * r + verticalVelocity * Vector3.up;
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
    }
}

