using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public static class SettlementPreferencePatch
    {
        [HarmonyPatch(typeof(TileFinder))]
        [HarmonyPatch("RandomSettlementTileFor")]
        public static class TileFinder_RandomSettlementTileFor_Patch
        {
            [HarmonyPrefix]
            public static bool SettlementPatch(ref int __result, Faction faction) => !MedievalOverhaulSettings.settings.StandaloneSettlementPreference_EnableSettlementPreference || faction == null || SettlementPreference.Get((Def)faction.def) == null || !Rand.Chance(SettlementPreference.Get((Def)faction.def).chance) || SettlementPreferenceUtility.GetTileID(faction, out __result);
        }
    }
}
