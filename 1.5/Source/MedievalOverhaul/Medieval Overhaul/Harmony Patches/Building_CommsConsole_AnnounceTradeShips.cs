using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Building_CommsConsole), nameof(Building_CommsConsole.AnnounceTradeShips))]
    public static class Building_CommsConsole_AnnounceTradeShips
    {
        public static bool Prefix(ref Building_CommsConsole __instance)
        {
            if (__instance is Building_ScribeTable)
            {
                return false;
            }
            return true;
        }
    }
}
