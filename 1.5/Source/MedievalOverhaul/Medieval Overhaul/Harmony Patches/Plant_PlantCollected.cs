using HarmonyLib;
using RimWorld;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
    public static class Plant_PlantCollected
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.soilWear;
        }
        public static void Prefix(Plant __instance, out Map __state)
        {
            __state = __instance.Map;
        }
        public static void Postfix(Plant __instance, Map __state)
        {
            var terrainGrid = __state.terrainGrid;
            if (terrainGrid.TerrainAt(__instance.Position).defName == "DankPyon_PlowedSoil")
            {

                if (Rand.Chance(MedievalOverhaulSettings.settings.soilWearChance))
                {
                    terrainGrid.RemoveTopLayer(__instance.Position);
                    if (MedievalOverhaulSettings.settings.autoPlow)
                    {
                        GenConstruct.PlaceBlueprintForBuild(MedievalOverhaulDefOf.DankPyon_PlowedSoil, __instance.Position, __state, Rot4.North, Faction.OfPlayer, (ThingDef)null);
                    }
                }
            }
            
        }
    }
}
