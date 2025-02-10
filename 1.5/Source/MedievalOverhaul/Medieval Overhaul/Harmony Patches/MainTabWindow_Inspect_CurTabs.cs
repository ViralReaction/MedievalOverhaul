using HarmonyLib;
using ProcessorFramework;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), nameof(MainTabWindow_Inspect.CurTabs), MethodType.Getter)]
    public class MainTabWindow_Inspect_CurTabs
    {
        [HarmonyPostfix]
        public static void CurTabs_Postfix(ref IEnumerable<InspectTabBase> __result)
        {
            List<object> objects = Find.Selector.SelectedObjects;
            if (!objects.NullOrEmpty())
            {
                Thing firstThing = objects.First() as Thing;
                if (objects.All(x => x is Thing thing && thing.Faction == Faction.OfPlayerSilentFail && thing.TryGetComp<CompStoreFuelThing>() != null
            && thing.def == firstThing.def))
                {
                    Thing thing = Find.Selector.SelectedObjects.First() as Thing;
                    __result = thing.GetInspectTabs();
                }
            }
        }
    }
}
