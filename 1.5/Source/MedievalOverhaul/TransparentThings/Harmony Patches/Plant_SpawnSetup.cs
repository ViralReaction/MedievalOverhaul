using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TransparentThings
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.SpawnSetup))]
    public class Plant_SpawnSetup
    {
        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }
        private static void Postfix(Plant __instance)
        {
            if (__instance.def.plant?.IsTree == true)
            {
                __instance.Map.GetComponent<TreeTransparencyGrid>().AddTree(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(Plant), nameof(Plant.DeSpawn))]
    public class Plant_DeSpawn
    {
        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }
        private static void Prefix(Plant __instance)
        {
            if (__instance.def.plant?.IsTree == true)
            {
                __instance.Map.GetComponent<TreeTransparencyGrid>().RemoveTree(__instance);
            }
        }
    }
}
