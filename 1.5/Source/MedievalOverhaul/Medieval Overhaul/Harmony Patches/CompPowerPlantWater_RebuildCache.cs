using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompPowerPlantWater), nameof(CompPowerPlantWater.RebuildCache))]
    public static class CompPowerPlantWater_RebuildCache
    {
        static CompPowerPlantWater_RebuildCache()
        {
        }

        public static void Postfix(CompPowerPlantWater __instance)
        {
            List<Building> waterWheel = __instance.parent.Map.listerBuildings.AllBuildingsColonistOfDef(MedievalOverhaulDefOf.DankPyon_WaterMill);
            foreach (IntVec3 waterCell in __instance.WaterUseCells())
            {
                if (waterCell.InBounds(__instance.parent.Map))
                {
                    foreach (Building building in waterWheel)
                    {
                        if (building != __instance.parent && building.GetComp<CompPowerWaterWheel>().WaterUseRect().Contains(waterCell))
                        {
                            __instance.waterDoubleUsed = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
