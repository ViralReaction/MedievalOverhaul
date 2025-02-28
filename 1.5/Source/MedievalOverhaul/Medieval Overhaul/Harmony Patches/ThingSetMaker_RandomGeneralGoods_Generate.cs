using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch]
    public class ThingSetMaker_RandomGeneralGoods_Generate
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.leatherChain;
        }
        public static MethodBase TargetMethod()
        {
            // use normal reflection or helper methods in <AccessTools> to find the method/constructor
            // you want to patch and return its MethodInfo/ConstructorInfo
            //
            return AccessTools.Method(typeof(ThingSetMaker_RandomGeneralGoods), nameof(ThingSetMaker_RandomGeneralGoods.Generate));
        }

        [HarmonyPostfix]
        protected static void Postfix(ref List<Thing> __result)
        {

            if (__result != null)
            {
                for (int i = 0; i < __result.Count; i++)
                {
                    if (HideUtility.IsHide(__result[i].def))
                    {
                        Thing thing = __result[i];
                        var comp = thing.TryGetComp<CompGenericHide>();
                        if (comp != null)
                        {
                            int num1 = thing.stackCount;
                            int parts = num1 / 50;
                            int remainder = num1 % 50;
                            if (parts > 0)
                            {
                                thing.stackCount = parts;
                                comp.leatherAmount = 50 + remainder / parts;
                            }
                            else
                            {
                                thing.stackCount = 1;
                                comp.leatherAmount = num1;

                            }
                            var leatherCost = comp.Props.leatherType.GetStatValueAbstract(StatDefOf.MarketValue);
                            comp.marketValue = (int)((int)(comp.leatherAmount * leatherCost) * 0.8f);
                            comp.massValue = (comp.leatherAmount * comp.Props.leatherType.GetStatValueAbstract(StatDefOf.Mass));
                        }
                    }
                }
            }
        }
    }
}