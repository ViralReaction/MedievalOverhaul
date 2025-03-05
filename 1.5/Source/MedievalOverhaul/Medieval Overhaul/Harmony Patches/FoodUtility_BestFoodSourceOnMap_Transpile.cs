using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    public static class FoodUtility_BestFoodSourceOnMap_HelperFind
    {
        
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
        private static Type DiningSpot_DispensedDef_Patch
        {
            get
            {
                return AccessTools.TypeByName("Gastronomy.Dining.DiningSpot");
            }
        }
        public static bool Prepare()
        {
            if (DiningSpot_DispensedDef_Patch != null)
            {
                Log.Warning("Medieval Overhaul: Detected Gastronomy - Disabling Food Preference Patch For Compatibility");
            }
            return DiningSpot_DispensedDef_Patch == null;
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
            bool foundGetPowerOn = false;
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

                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo { Name: "get_PowerOn" })
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(FoodUtility_BestFoodSourceOnMap_Transpile), nameof(GetPowerOn_Replacement));

                    foundGetPowerOn = true;
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

            if (!foundGetPowerOn)
            {
                Log.Error("[Harmony] MO.FoodUtility.BestFoodSourceOnMap: Could not find any occurrence of get_PowerOn!");
            }

            return codes;
        }

        private static bool GetPowerOn_Replacement(CompPowerTrader comp)
        {
            return CompPowerTrader_PowerOn.Postfix(comp.powerOnInt, comp);
        }
    }

}