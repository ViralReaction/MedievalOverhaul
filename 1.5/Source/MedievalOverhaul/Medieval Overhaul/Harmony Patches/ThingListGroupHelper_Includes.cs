using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;
using System.Reflection;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]

    public static class ThingListGroupHelper_Includes
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            MethodInfo equalityMethod = AccessTools.Method(typeof(Type), "op_Equality", new[] { typeof(Type), typeof(Type) });
            MethodInfo customCheckMethod = AccessTools.Method(typeof(ThingListGroupHelper_Includes), nameof(TypeCheck));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(equalityMethod))
                {
                    // Check if the previous instruction loads `Building_NutrientPasteDispenser`
                    if (i >= 2 && code[i - 1].opcode == OpCodes.Call &&
                        code[i - 2].opcode == OpCodes.Ldtoken &&
                        code[i - 2].operand is Type targetType &&
                        targetType == typeof(Building_NutrientPasteDispenser))
                    {
                        code[i] = new CodeInstruction(OpCodes.Call, customCheckMethod);
                    }
                }
            }
            return code;
        }

        public static bool TypeCheck(Type thingClass, Type targetType)
        {
            return thingClass == targetType || thingClass.IsSubclassOf(targetType);
        }
    }
}
