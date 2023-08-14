using UnityEngine;

namespace GameStationData
{
    public class GameStationData : NetworkSceneTool
    {
        public int MinRequiredPlayers;
        public int MaxPlayers;
        public GameSessionType GameType;
        public Transform countingAreaPosition;
        public GameObject GameLeaderBoard;
        public GameObject GameStartButton;
        public bool AutoStart;
        public float GameTimerInSec;
    }
    public enum GameSessionType
    {
        CannonGame,
        RunnerGame,
        AliceInBorderlandGame,
        QuizzGame,
        ShiFuMiTournamentGame,
        WereWolfGame,
        TotoGame,
    }
}
