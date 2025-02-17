using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    public static class FoodUtility_BestFoodSourceOnMap_HelperFind
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        public static MethodInfo AnonymousMethod;
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);

            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldftn && codes[i + 1].opcode == OpCodes.Newobj)
                {
                    // Capture the function reference of <BestFoodSourceOnMap>b__0
                    AnonymousMethod = (MethodInfo)codes[i].operand;
                    break;
                }
            }

            return codes;
        }
    }
    [HarmonyPatch]
    public static class FoodUtility_BestFoodSourceOnMap_Transpile
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        static MethodBase TargetMethod()
        {
            if (FoodUtility_BestFoodSourceOnMap_HelperFind.AnonymousMethod != null)
            {
                return FoodUtility_BestFoodSourceOnMap_HelperFind.AnonymousMethod;
            }
            throw new Exception("MO.FoodUtility.BestFoodSourceOnMap:  Could not find the anonymous function!");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            int injectionCount = 0;

            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo field &&
                    field.Name == "MealNutrientPaste")
                {
                    #if DEBUG
                    Log.Message($"[Harmony] Replacing occurrence {injectionCount + 1} of MealNutrientPaste with Building_NutrientPasteDispenser.DispensableDef");
                    #endif

                    codes[i] = new CodeInstruction(OpCodes.Ldloc_1); // Load stored `Building_NutrientPasteDispenser`
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Building_NutrientPasteDispenser), "DispensableDef")));

                    injectionCount++;
                }
            }

            if (injectionCount == 0)
            {
                Log.Error("[Harmony] MO.FoodUtility.BestFoodSourceOnMap: Could not find any occurrences of MealNutrientPaste!");
            }
            #if DEBUG
            else
            {
                Log.Message($"[Harmony] Successfully replaced {injectionCount} occurrences of MealNutrientPaste.");
            }
            #endif

            return codes;
        }
    }

}