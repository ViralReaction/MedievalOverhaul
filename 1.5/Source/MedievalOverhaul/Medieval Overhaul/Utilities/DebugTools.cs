using Verse;
using LudeonTK;

namespace MedievalOverhaul
{
    public static class DebugTools
    {
        [DebugAction("MedievalOverhaul", "MOTest", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]

        public static void MOTest(Pawn p)
        {
            UI.MouseCell();
            Log.Message(p.LabelShort);
        }
    }
}
