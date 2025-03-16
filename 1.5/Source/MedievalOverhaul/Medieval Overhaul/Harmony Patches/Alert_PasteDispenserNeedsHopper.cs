using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(Alert_PasteDispenserNeedsHopper), "BadDispensers", MethodType.Getter)]
    public static class Alert_BadDispensers
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        public static List <Thing> Postfix(List<Thing> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                if (__result[i] is Building_SlopPot)
                {
                    __result.RemoveAt(i);
                }
            }
            return __result;
        }
    }
}
