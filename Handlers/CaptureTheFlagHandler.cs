using RWF.GameModes;
using CaptureTheFlag.GameMode;
namespace CaptureTheFlag.GameModeHandler
{
    public class CaptureTheFlagHandler : RWFGameModeHandler<GM_CaptureTheFlag>
    {
        internal const string GameModeName = "Capture The Flag";
        internal const string GameModeID = "Capture The Flag";
        public CaptureTheFlagHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: true,
            pointsToWinRound: 5,
            roundsToWinGame: 3,
            // null values mean RWF's instance values
            playersRequiredToStartGame: 2,
            maxPlayers: 8,
            maxTeams: 2,
            maxClients: null)
        {

        }
    }
}