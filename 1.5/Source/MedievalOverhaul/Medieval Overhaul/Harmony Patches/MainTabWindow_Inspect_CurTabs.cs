﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), nameof(MainTabWindow_Inspect.CurTabs), MethodType.Getter)]
    public class MainTabWindow_Inspect_CurTabs
    {
        private static List<object> _cachedSelectedObjects = new();
        private static IEnumerable<InspectTabBase> _cachedResult;

        [HarmonyPostfix]
        public static void CurTabs_Postfix(ref IEnumerable<InspectTabBase> __result)
        {
            List<object> objects = Find.Selector.SelectedObjects;
            if (objects == null || objects.Count == 0) return;
            
            int objectCount = objects.Count;
            if (_cachedSelectedObjects.Count == objectCount)
            {
                if (_cachedResult == null) return;
                bool sameSelection = true;
                for (int i = 0; i < objectCount; i++)
                {
                    if (_cachedSelectedObjects[i] != objects[i]) // Compare by reference
                    {
                        sameSelection = false;
                        break;
                    }
                }
                if (sameSelection)
                {
                    __result = _cachedResult;
                    return;
                }
            }
            _cachedSelectedObjects.Clear();
            _cachedSelectedObjects.AddRange(objects);
            _cachedResult = null;
            if (objects[0] is not ThingWithComps firstThing || firstThing.Faction != Faction.OfPlayerSilentFail)
            {
                return;
            }
            for (int i = 1; i < objects.Count; i++)
            {
                if (objects[i] is not ThingWithComps thing || thing.Faction != Faction.OfPlayerSilentFail || thing.def != firstThing.def)
                {
                    return;
                }
            }
            var thingCompList = firstThing.AllComps;
            if (thingCompList.Count == 0) return;
            for (int i = 0; i < thingCompList.Count; i++)
            {
                if (thingCompList[i] is ICompFuelHandler)
                {
                    _cachedResult = firstThing.GetInspectTabs();
                    __result = _cachedResult;
                    return;
                }
            }
        }

    }
}
