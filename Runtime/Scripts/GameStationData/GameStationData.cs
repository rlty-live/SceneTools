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
        public GameObject GameBanner;
        public GameObject ReturnPosition;
        public bool AutoStart;
        public int GameTimerInSec;
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
        BattleRoyale,
        DeathrunGame,
        DeathmatchGame,
        RaceGame
    }
}
