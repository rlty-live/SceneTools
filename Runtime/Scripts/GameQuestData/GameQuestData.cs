using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameQuestSystem
{
    public enum GameQuestType
    {
        Farm,
    }
    
    public enum FarmQuestCompletionType
    {
        FarmAllChests,
        FarmJustOne
    }
    
    public enum FarmQuestRewardType
    {
        RewardPerChest,
        RewardAfterAllChests
    }
    
    public class GameQuestData : NetworkSceneTool
    {
        public GameQuestType QuestType;
        public int WinnerCoinAmount;
        
        // Farm Quest
        [ShowIf("QuestType", GameQuestType.Farm)]
        public List<Transform> ChestSpawnPositions = new List<Transform>();
        [ShowIf("QuestType", GameQuestType.Farm)]
        [Min(1)]
        public int AmountOfChestsToSpawn;
        [ShowIf("QuestType", GameQuestType.Farm)]
        public FarmQuestCompletionType QuestCompletionType;
        [ShowIf("QuestType", GameQuestType.Farm)]
        public FarmQuestRewardType QuestRewardType;
        public bool QuestVisibleToActivatorOnly;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (ChestSpawnPositions == null || ChestSpawnPositions.Count == 0)
            {
                Handles.Label(transform.position + Vector3.up, "Waiting for Start Transform...");

                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
            Handles.Label(transform.position + Vector3.up, $"{QuestType} Quest");

            foreach (Transform chestSpawn in ChestSpawnPositions)
            {
                Gizmos.DrawSphere(chestSpawn.position, 0.2f);
                Handles.Label(chestSpawn.position + Vector3.up, $"{QuestType} Quest");
            }
        }
#endif
    }

}