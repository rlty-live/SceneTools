using UnityEditor;
using UnityEngine;

namespace GameQuestSystem
{
    public class GameQuestSpawnPoint : MonoBehaviour
    {
        public GameQuestType QuestType;
        
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.green;
            if (QuestType == GameQuestType.Farm)
            {
                Vector3 lineStart = (transform.position + Vector3.up * 0.5f) + (transform.forward * 0.5f);
                Gizmos.DrawLine(lineStart, lineStart + (transform.forward * 0.5f));
                
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.up * 0.5f, new Vector3(2, 1, 1));
            }
#endif
        }
    }
}