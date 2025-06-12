using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Reflection;

namespace MedievalOverhaul.Patches
{
    public static class ResearchProjectsDefs
    {

        [HarmonyPatch]
        public class ResearchProjectDef_CanStartNow
        {
            public static bool Prepare()
            {
                if (ModsConfig.BiotechActive)
                {
                    return !MedievalOverhaulSettings.settings.biotechSchematic;
                }
                return true;
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(typeof(ResearchProjectDef), "CanStartNow");
            }
            private static bool? cachedSchematicCheck;
            private static int cacheStaleAfterTicks = -1;
            private const int CacheDuration = 250;
            public static void Postfix(ResearchProjectDef __instance, ref bool __result)
            {
                if (__result)
                {
                    var extension = __instance.GetModExtension<RequiredSchematic>();
                    if (extension != null && !PlayerHasSchematic(__instance, extension))
                    {
                        __result = false;
                    }
                }
            }
            private static bool PlayerHasSchematic(ResearchProjectDef __instance, RequiredSchematic extension)
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    List<Building> allBuildingsColonist = maps[i].listerBuildings.allBuildingsColonist;
                    for (int j = 0; j < allBuildingsColonist.Count; j++)
                    {
                        if (allBuildingsColonist[j] is Building_ResearchBench bench && HasBook(bench, extension))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private static bool HasBook(Building_ResearchBench bench, RequiredSchematic extension)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (cacheStaleAfterTicks != -1 && cachedSchematicCheck.HasValue && currentTick - cacheStaleAfterTicks < CacheDuration)
                {
                    return cachedSchematicCheck.Value;
                }
                Room room = bench.GetRoom();
                foreach (var thing in room.uniqueContainedThings)
                {
                    if (thing is not Building_Bookcase bookCase) continue;
                    foreach (var book in bookCase.HeldBooks)
                    {
                        if (book.def != extension.schematicDef) continue;
                        cachedSchematicCheck = true;
                        cacheStaleAfterTicks = currentTick;
                        return true;
                    }
                }
                cachedSchematicCheck = false;
                cacheStaleAfterTicks = currentTick;
                return false;
            }
        }
    }
}