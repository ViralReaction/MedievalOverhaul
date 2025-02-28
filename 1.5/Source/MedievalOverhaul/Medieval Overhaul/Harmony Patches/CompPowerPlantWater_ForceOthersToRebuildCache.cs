using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompPowerPlantWater), nameof(CompPowerPlantWater.ForceOthersToRebuildCache))]
    public static class CompPowerPlantWater_ForceOthersToRebuildCache
    {
        static CompPowerPlantWater_ForceOthersToRebuildCache()
        {
        }

        public static void Postfix(Map map)
        {
            foreach (Building building in map.listerBuildings.AllBuildingsColonistOfDef(MedievalOverhaulDefOf.DankPyon_WaterMill))
            {
                var comp = building.TryGetComp<CompPowerWaterWheel>();
                comp?.ClearCache();
            }
        }
    }

}