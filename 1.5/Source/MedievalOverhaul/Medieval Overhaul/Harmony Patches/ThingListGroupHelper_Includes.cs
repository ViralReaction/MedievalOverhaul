﻿using HarmonyLib;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class ThingListGroupHelper_Includes
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        [HarmonyPrefix]
        public static bool Prefix(this ThingRequestGroup group, ThingDef def, ref bool __result)
        {
            switch (group)
            {
                case ThingRequestGroup.FoodSource when def.IsFoodDispenser:
                    __result = true;
                    return false;
                case ThingRequestGroup.FoodSourceNotPlantOrTree when def.IsFoodDispenser:
                    __result = true;
                    return false;
                //case ThingRequestGroup.Refuelable when def.HasComp(typeof(CompRefuelableCustom)):
                //    __result = true;
                //    return false;
                default:
                    return true;
            }
        }
    }
}
