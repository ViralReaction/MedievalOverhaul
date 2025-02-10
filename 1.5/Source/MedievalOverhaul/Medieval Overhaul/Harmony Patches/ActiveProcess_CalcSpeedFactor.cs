using HarmonyLib;
using Verse;
using ProcessorFramework;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ActiveProcess), "CalcSpeedFactor")]
    public static class ActiveProcess_CalcSpeedFactor
    {
        public static void Postfix(ref float __result, ActiveProcess __instance, CompProcessor ___processor)
        {
            float fuelFactor = CurrentFuelFactor(___processor);
            __result *= fuelFactor;
        }

        public static float CurrentFuelFactor(CompProcessor ___processor)
        {
            if (___processor?.parent == null)
            {
                return 1f;
            }

            var comp = ___processor.parent.TryGetComp<CompPowerWaterWheel>();
            if (comp != null)
            {
                return comp.WheelProcessSpeed;
            }

            return 1f;
        }
    }
}