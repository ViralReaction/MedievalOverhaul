using HarmonyLib;
using ProcessorFramework;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(ActiveProcess), "CurrentWindFactor", MethodType.Getter)]
    public static class ActiveProcess_CurrentWindFactor
    {
        public static void Postfix(ref float __result, CompProcessor ___processor)
        {
            float millSpeed = CurrentSpinSpeed(___processor);
            __result *= millSpeed;
        }
        public static float CurrentSpinSpeed(CompProcessor ___processor)
        {
            if (___processor?.parent == null)
            {
                return 1f;
            }

            var comp = ___processor.parent.TryGetComp<Comp_WindMill>();
            if (comp != null)
            {
                return comp.BlockageFactor;
            }

            return 1f;
        }
    }
}