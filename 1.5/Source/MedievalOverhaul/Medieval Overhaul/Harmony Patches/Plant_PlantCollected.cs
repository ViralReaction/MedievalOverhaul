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

        public static void Prefix(Plant __instance, out Map __state)
        {
            __state = __instance.Map;
        }
        public static void Postfix(Plant __instance, Map __state)
        {
            var terrainGrid = __state.terrainGrid;
            if (terrainGrid.TerrainAt(__instance.Position).defName == "DankPyon_PlowedSoil")
            {
                //var comp = map.GetComponent<PlowSoilManager>();
                //var thingDef = comp.DeregisterSoil(__instance.Position);
                terrainGrid.RemoveTopLayer(__instance.Position);
                GenConstruct.PlaceBlueprintForBuild(MedievalOverhaulDefOf.DankPyon_PlowedSoil, __instance.Position, __state, Rot4.North, Faction.OfPlayer, (ThingDef)null);
            }
            
        }
    }
}
