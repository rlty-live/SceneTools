using Sirenix.OdinInspector;
using UnityEngine;

namespace GameStationData
{
    public class WeaponSpawner : SceneTool
    {
        public bool SpawnRandomWeapon;
        [ShowIf("@this.SpawnRandomWeapon == false")]
        public int WeaponId;

        protected override void DrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0);
            Matrix4x4 DefaultMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0.1f, 1f));
            Gizmos.DrawWireCube(Vector3.up, new Vector3(0.2f, 1f, 0.2f));
            Gizmos.matrix = DefaultMatrix;
        }
    }
}