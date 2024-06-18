using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils
{
    public static class GameManagerCommands
    {
        [Command("/game.round.win", "Force the team to won the current round")]
        public static void ForceRoundWin(PlayerTeamType teamType)
        {
            if (teamType == PlayerTeamType.Z)
            {
                Logger.LogError("You can't force the Z team to win the round !");
                return;
            }
            if(GameManager.HasInstance) GameManager.Instance.TryForceRoundWinner(teamType);
        }

        [Command("/game.reset", "Reset the game")]
        public static void ResetGame()
        {
            if (GameManager.HasInstance) GameManager.Instance.ResetGame();
        }
        
        [Command("/game.win", "Force the team to win the game")]
        public static void ForceGameWin(PlayerTeamType teamType)
        {
            if (teamType == PlayerTeamType.Z)
            {
                Logger.LogError("You can't force the Z team to win the game !");
                return;
            }
            if(GameManager.HasInstance) GameManager.Instance.TryForceGameWinner(teamType);
        }
    }
}