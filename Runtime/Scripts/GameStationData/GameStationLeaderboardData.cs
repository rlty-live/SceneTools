namespace GameStationData
{
    public enum LeaderboardDataType
    {
        Other,
        Wins,
        Losses,
    }
    
    public class GameStationLeaderboardData : SceneTool
    {
        public GameSessionType LeaderboardGameSessionType;
        
        public LeaderboardDataType LeaderboardDataType;
    }
}