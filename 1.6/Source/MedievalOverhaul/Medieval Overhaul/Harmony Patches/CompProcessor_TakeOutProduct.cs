using Verse;
using HarmonyLib;
using ProcessorFramework;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompProcessor), "TakeOutProduct")]
    public static class CompProcessor_TakeOutProduct
    {
        public static bool Prefix(CompProcessor __instance, ref Thing __result, ActiveProcess activeProcess)
        {
            var extension = activeProcess.processDef.GetModExtension<ProcessorExtension>();
            if (extension?.outputOnlyButcherProduct == true && activeProcess.ingredientThings != null)
            {
                foreach (var thing in activeProcess.ingredientThings)
                {
                    if (thing != null && TryGetButcherProduct(thing, out var thingDefCount))
                    {
                        __result = TakeOutButcherProduct(__instance, thing, thingDefCount, activeProcess);
                        break;
                    }
                }
                return false;
            }
            return true;
        }
        public static void Postfix(ref Thing __result, ActiveProcess activeProcess)
        {
            if (activeProcess.processDef == MedievalOverhaulDefOf.DankPyon_RawHidesProcess)
            {
                foreach (var thing in activeProcess.ingredientThings)
                {
                    var comp = thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        __result.stackCount = comp.leatherAmount;
                    }
                }
            }
        }

        private static bool TryGetButcherProduct(Thing thing, out ThingDefCountClass result)
        {
            result = null;
            var comp = thing.TryGetComp<CompGenericHide>();
            if (comp != null)
            {
                int count = comp.leatherAmount;
                result = new ThingDefCountClass(comp.Props.leatherType, count);
                return true;
            }

            if (thing.def.butcherProducts?.Count > 0)
            {
                result = thing.def.butcherProducts[0];
                return true;
            }
            return false;
        }

        public static Thing TakeOutButcherProduct(CompProcessor __instance, Thing ingredientThing, ThingDefCountClass thingDefCount, ActiveProcess activeProcess)
        {
            Thing product = MakeProductFromDefCount(thingDefCount, activeProcess.ingredientCount);

            RemoveAndDestroyIngredient(__instance, ingredientThing, activeProcess);
            FinishProcessIfNoIngredients(__instance, activeProcess);

            return product;
        }

        private static Thing MakeProductFromDefCount(ThingDefCountClass thingDefCount, int ingredientCount)
        {
            var thing = ThingMaker.MakeThing(thingDefCount.thingDef);
            thing.stackCount = thingDefCount.count * ingredientCount;
            return thing;
        }

        private static void RemoveAndDestroyIngredient(CompProcessor __instance, Thing ingredientThing, ActiveProcess activeProcess)
        {
            __instance.innerContainer.Remove(ingredientThing);
            ingredientThing.Destroy();
            activeProcess.ingredientThings.Remove(ingredientThing);
        }

        private static void FinishProcessIfNoIngredients(CompProcessor __instance, ActiveProcess activeProcess)
        {
            if (!activeProcess.ingredientThings.Any())
            {
                __instance.activeProcesses.Remove(activeProcess);
                if (__instance.Empty)
                    __instance.GraphicChange(true);
                if (!__instance.activeProcesses.Any(x => x.processDef.usesQuality))
                    __instance.emptyNow = false;
            }
        }
    }
}
