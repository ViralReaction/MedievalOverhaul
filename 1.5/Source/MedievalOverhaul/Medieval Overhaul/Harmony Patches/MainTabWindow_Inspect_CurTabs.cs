using HarmonyLib;
using ProcessorFramework;
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
            if (objects == null || objects.Count == 0)
            {
                return;
            }
            if (_cachedSelectedObjects.Count == objects.Count)
            {
                bool sameSelection = true;
                for (int i = 0; i < objects.Count; i++)
                {
                    if (_cachedSelectedObjects[i] != objects[i]) // Compare by reference
                    {
                        sameSelection = false;
                        break;
                    }
                    if (sameSelection)
                    {
                        if (_cachedResult != null)
                        {
                            __result = _cachedResult;
                        }
                        return;
                    }
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
            if (firstThing.AllComps.Count == 0) return;
            for (int i = 0; i < firstThing.AllComps.Count; i++)
            {
                if (firstThing.AllComps[i] is ICompFuelHandler)
                {
                    _cachedResult = firstThing.GetInspectTabs();
                    __result = _cachedResult;
                    return;
                }
            }
        }

    }
}
