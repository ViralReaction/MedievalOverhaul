using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

    public static class Harmony_Gastronomy
    {
        private static Type DiningSpot_DispensedDef_Patch
        {
            get
            {
                return AccessTools.TypeByName("Gastronomy.Dining.DiningSpot");
            }
        }
        [HarmonyPatch]
        public static class Harmony_CompSwapWeapons_Apply
        {
            public static bool Prepare()
            {
                return DiningSpot_DispensedDef_Patch != null;
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter("Gastronomy.Dining.DiningSpot:DispensableDef");
            }

            public static bool Prefix(ref ThingDef __result)
            {

                __result = ThingDefOf.MealNutrientPaste;
                return false;
            }
        }

    }
}
