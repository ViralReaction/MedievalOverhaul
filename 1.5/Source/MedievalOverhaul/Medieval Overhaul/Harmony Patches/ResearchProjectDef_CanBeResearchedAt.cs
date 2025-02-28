﻿using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch]
    public class ResearchProjectDef_CanBeResearchedAt
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
            return AccessTools.Method(typeof(ResearchProjectDef), "CanBeResearchedAt");
        }

        private static bool? cachedSchematicCheck;
        private static int cacheStaleAfterTicks = -1;
        private const int CacheDuration = 250;
        public static void Postfix(Building_ResearchBench bench, ResearchProjectDef __instance, ref bool __result)
        {
            if (__result)
            {
                var extension = __instance.GetModExtension<RequiredSchematic>();
                if (extension != null && HasBook(bench, extension) is false)
                {
                    __result = false;
                }
            }
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
                if (thing is Building_Bookcase bookCase)
                {
                    foreach (var book in bookCase.HeldBooks)
                    {
                        if (book.def == extension.schematicDef)
                        {
                            cachedSchematicCheck = true;
                            cacheStaleAfterTicks = currentTick;
                            return true;
                        }
                    }
                }
            }
            cachedSchematicCheck = false;
            cacheStaleAfterTicks = currentTick;
            return false;
        }
    }
}
