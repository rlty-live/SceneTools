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
        [ShowIf("QuestType", GameQuestType.Farm), HideIf("QuestCompletionType", FarmQuestCompletionType.FarmJustOne)]
        public FarmQuestRewardType QuestRewardType;
        [ShowIf("QuestType", GameQuestType.Farm)]
        public bool TimedQuest;
        [ShowIf("@this.QuestType == GameQuestType.Farm && this.TimedQuest")]
        [Tooltip("If a player doesn't interact with a chest within this time, the quest will fail. 0 = no time limit")]
        public float MaxTimePerChestInSeconds;
        [ShowIf("@this.QuestType == GameQuestType.Farm && this.TimedQuest && this.QuestCompletionType != FarmQuestCompletionType.FarmJustOne")]
        [Tooltip("If all chests aren't interacted with within this time, the quest will fail. 0 = no time limit")]
        public float MaxTimeAllChestsInSeconds;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color32 color = Color.green;
            string label = "";
            if (ChestSpawnPositions == null || ChestSpawnPositions.Count == 0)
            {
                label = "No chest spawn positions...";
                color = Color.red;
            }
            else
            {
                label = $"{QuestType} Quest";
            }
            Handles.Label(transform.position + Vector3.up, label);
            color.a = 255;

            Color32 colorSeeThrough = color;
            colorSeeThrough.a = 64;
            
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 size = new Vector3(1f, 1f, 0.2f);
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.color = colorSeeThrough;
            Gizmos.DrawCube(Vector3.zero, size);
            Gizmos.matrix = previousMatrix;
        }
#endif
    }

}