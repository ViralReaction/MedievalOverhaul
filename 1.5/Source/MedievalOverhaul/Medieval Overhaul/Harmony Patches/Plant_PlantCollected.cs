using HarmonyLib;
using RimWorld;
using Verse;


namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
    public static class Plant_PlantCollected
    {
        public static float TerrainFertilityMax => MedievalOverhaulDefOf.DankPyon_PlowedSoil.fertility;
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
            if (__instance.HarvestableNow)
            {
                var terrainGrid = __state.terrainGrid;
                var position = __instance.Position;
                if (terrainGrid.TerrainAt(position).defName == "DankPyon_PlowedSoil")
                {

                    if (Rand.Chance(MedievalOverhaulSettings.settings.soilWearChance))
                    {
                        terrainGrid.RemoveTopLayer(position);
                        if (MedievalOverhaulSettings.settings.autoPlow && TerrainFertilityMax >= terrainGrid.TerrainAt(position).fertility)
                        {
                            GenConstruct.PlaceBlueprintForBuild(MedievalOverhaulDefOf.DankPyon_PlowedSoil, position, __state, Rot4.North, Faction.OfPlayer, (ThingDef)null);
                        }
                    }
                }

            }
        }
    }

}