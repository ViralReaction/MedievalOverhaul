using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ThingDefGenerator_Buildings), nameof(ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs))]
    public static class ThingDefGenerator_Buildings_ImpliedBlueprintAndFrameDefs
    {

        private static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result, bool hotReload)
        {
            List<ThingDef> defList = [];
            foreach (ThingDef thingDef in __result)
            {
                defList.Add(thingDef);
            }
            ThingDef def = MedievalOverhaulDefOf.DankPyon_CultBook;
            if (def != null)
            {
                ThingDef blueprint = ThingDefGenerator_Buildings.NewBlueprintDef_Thing(def, false, null, hotReload);
                defList.Add(blueprint);
                ThingDef frameDef = ThingDefGenerator_Buildings.NewFrameDef_Thing(def, hotReload);
                defList.Add(frameDef);
            }

            IEnumerable<ThingDef> output = defList;
            __result = output;
            return __result;
        }
    }
}