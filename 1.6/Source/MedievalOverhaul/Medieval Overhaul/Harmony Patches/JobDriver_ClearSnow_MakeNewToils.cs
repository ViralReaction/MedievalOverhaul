using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(JobDriver_ClearSnowAndSand), nameof(JobDriver_ClearSnowAndSand.MakeNewToils))]
    public static class JobDriver_ClearSnow_MakeNewToils
    {
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver __instance)
        {
            List<Toil> toils = new List<Toil>(__result); // Convert enumerable to list for modification
            if (toils.Count < 2) return; // Ensure there's a valid toil list

            Toil clearToil = toils[1]; // The actual snow clearing toil

            clearToil.AddFinishAction(delegate
            {
                if (Rand.Chance(0.1f)) // 10% chance to create an ice block
                {
                    IntVec3 cell = __instance.TargetLocA;
                    Map map = __instance.Map;
                    if (IsSand(map, cell)) return;
                    Thing iceBlock = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_IceBlock);
                    GenPlace.TryPlaceThing(iceBlock, cell, map, ThingPlaceMode.Near);
                }
            });
            __result = (IEnumerable<Toil>)toils; // Convert list back to IEnumerable<Toil>
        }
        private static bool IsSand(Map map, IntVec3 cell)
        {
            float? num = map.sandGrid?.GetDepth(cell);
            if (num.HasValue)
            {
                return num.GetValueOrDefault() > map.snowGrid.GetDepth(cell);
            }
            return false;
        }

    }
}