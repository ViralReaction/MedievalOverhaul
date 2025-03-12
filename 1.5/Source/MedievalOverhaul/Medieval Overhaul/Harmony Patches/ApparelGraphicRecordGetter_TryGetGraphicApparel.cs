using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using RimWorld;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class ApparelGraphicRecordGetter_TryGetGraphicApparel
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            int targetIndex = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Beq_S && codes[i].operand is Label label)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex == -1)
            {
                Log.Error("MO.ApparelGraphicRecordGetter.TryGetGraphicApparel: Could not find Harmony injection site");
                return codes;
            }

            Label jumpLabel = (Label)codes[targetIndex].operand;

            List<CodeInstruction> newInstructions = new List<CodeInstruction>
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), "def")),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), "apparel")),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ApparelProperties), "get_LastLayer")),
            new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(MedievalOverhaul.MedievalOverhaulDefOf), "DankPyon_Hood")),
            new CodeInstruction(OpCodes.Beq_S, jumpLabel)
        };

            codes.InsertRange(targetIndex + 1, newInstructions);

            return codes;
        }
    }
}