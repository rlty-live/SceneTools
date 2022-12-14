using UnityEngine;

namespace Judiva.Metaverse.Interactions
{
    public class SpawnPoint : MonoBehaviour
    {

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1);
        }
    }
}
