using GameStationData;
using Sirenix.OdinInspector;

namespace GameSessionManager
{
    public class GameRuleSceneTool : SceneTool
    {
        public bool SetupSceneAsGameSession;
        [ShowIf("SetupSceneAsGameSession")]
        public GameSessionType ActiveGameSessionType;
        public bool WaitForMinimumPlayers;
    }
}