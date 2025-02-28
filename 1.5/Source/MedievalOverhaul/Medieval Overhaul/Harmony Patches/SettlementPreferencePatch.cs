using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor")]
    public static class SettlementPreferencePatch
    {

        public static bool Prefix(ref int __result, Faction faction)
        {
            // If the setting to enable settlement preference is disabled, return true
            if (!MedievalOverhaulSettings.settings.StandaloneSettlementPreference_EnableSettlementPreference || faction is null)
                return true;


            // Get the settlement preference for this faction definition
            var preference = SettlementPreference.Get((Def)faction.def);

            if (preference is null || !Rand.Chance(preference.chance))
            {
                return true;
            }

            // Attempt to get the tile ID; return its result
            return SettlementPreferenceUtility.GetTileID(faction, out __result);
        }
    }
}
