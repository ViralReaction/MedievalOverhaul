﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
    public static class GenConstruct_CanPlaceBlueprintOver
    {
        public static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
            foreach (IntVec3 cell in cellRect)
            {
                var thingList = cell.GetThingList(map);
                for (int m = 0; m < thingList.Count; m++)
                {
                    Thing thing2 = thingList[m];
                    var otherDef = thing2.def.IsBlueprint ? thing2.def.entityDefToBuild : thing2.def;
                    if (thing2 != thingToIgnore && otherDef.GetModExtension<CannotBePlacedTogetherWithThisModExtension>() != null
                        && entDef.GetModExtension<CannotBePlacedTogetherWithThisModExtension>() != null)
                    {
                        __result = new AcceptanceReport("SpaceAlreadyOccupied".Translate());
                        return;
                    }
                }
            }
        }
    }
}