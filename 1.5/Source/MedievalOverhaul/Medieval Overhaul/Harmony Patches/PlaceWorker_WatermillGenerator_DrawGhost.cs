using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(PlaceWorker_WatermillGenerator), "DrawGhost")]
    public static class Patch_PlaceWorker_WatermillGenerator_DrawGhost
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var originalAddRange = AccessTools.Method(typeof(List<Thing>), "AddRange");
            var modifiedAddRange = AccessTools.Method(typeof(Patch_PlaceWorker_WatermillGenerator_DrawGhost), nameof(CustomAddRange));

            List<CodeInstruction> codes = instructions.ToList();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(originalAddRange))
                {
                    // Replace List<Thing>.AddRange(IEnumerable<Thing>) with our custom method
                    codes[i] = new CodeInstruction(OpCodes.Call, modifiedAddRange);
                }
            }

            return codes;
        }

        // Custom method replacing AddRange calls
        public static void CustomAddRange(List<Thing> waterMills, IEnumerable<Thing> things)
        {

            // Use reflection to get private static field waterMills
            var waterMillsField = AccessTools.Field(typeof(PlaceWorker_WaterWheel), "waterMills");
            if (waterMillsField == null)
            {
                return;
            }

            var actualWaterMills = (List<Thing>)waterMillsField.GetValue(null);

            // Now modify actualWaterMills
            actualWaterMills.Clear();

            actualWaterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator).Cast<Thing>());
            actualWaterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(MedievalOverhaulDefOf.DankPyon_WaterMill).Cast<Thing>());
            actualWaterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                                                       where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                       select t);
            actualWaterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                                                       where t.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill
                                                       select t);
            actualWaterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
                                                       where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                       select t);
            actualWaterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
                                                       where t.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill
                                                       select t);
        }
    }
}