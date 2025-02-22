using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Building_CommsConsole), nameof(Building_CommsConsole.CanUseCommsNow), MethodType.Getter)]
    public static class Building_CommsConsole_CanUseCommsNow
    {
        public static bool Prefix(ref bool __result, Building_CommsConsole __instance)
        {
            if (__instance != null && __instance is Building_ScribeTable scribeTable)
            {
                __result = scribeTable.CanActuallyUseCommsNow;
                return false;
            }
            return true;
        }
    }
}
