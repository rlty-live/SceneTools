using UnityEngine;

namespace GameStationData
{
    [System.Serializable]
    public enum PlayerStatPickupType
    {
        Health,
        Shield
    }
    
    public class PlayerStatPickupSpawner : SceneTool
    {
        public PlayerStatPickupType PlayerStatPickupType;

        protected override void DrawGizmos()
        {
            Gizmos.color = PlayerStatPickupType == PlayerStatPickupType.Health ? new Color(1, 0.2f, 0.2f) : new Color(0, 1, 1);
            Matrix4x4 DefaultMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0.1f, 1f));
            Gizmos.DrawWireSphere(Vector3.up, 0.2f);
            Gizmos.matrix = DefaultMatrix;
        }
    }
}