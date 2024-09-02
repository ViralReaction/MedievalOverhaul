using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using VanillaGenesExpanded;
using Verse;
using VFECore;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
    public static class Pawn_ButcherProducts
    {
        private static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance, float efficiency)
        {
            List<Thing> productList = [];
            var pawn = __instance;

            // Adding normal butcher output to the list
            if (MedievalOverhaulSettings.settings.leatherChain)
            {
                foreach (var product in __result)
                {
                    if (HideUtility.IsHide(product.def))
                    {
                        var productThing = StaticCollectionsClass.leather_gene_pawns.TryGetValue(pawn, out var hideDef)? ThingMaker.MakeThing(hideDef): product;
                        productThing.stackCount = product.stackCount;
                        var comp = productThing.TryGetComp<CompGenericHide>();

                        if (comp != null)
                        {
                            comp.leatherAmount = product.stackCount;
                            var leatherCost = comp.Props.leatherType.GetStatValueAbstract(StatDefOf.MarketValue);
                            comp.marketValue = (int)(comp.leatherAmount * leatherCost * 0.8f);
                            comp.massValue = comp.leatherAmount * comp.Props.leatherType.GetStatValueAbstract(StatDefOf.Mass);
                            productThing.stackCount = 1;
                        }

                        productList.Add(productThing);
                    }
                    else
                    {
                        productList.Add(product);
                    }
                }
            }
            else
            {
                productList.AddRange(__result);
            }

            // Checking for additional butcher products and adding to list
            if (__instance.def.GetModExtension<AdditionalButcherProducts>() is { butcherOptions: var butcherList })
            {
                foreach (var option in butcherList)
                {
                    if (Rand.Chance(option.chance))
                    {
                        var product = ThingMaker.MakeThing(option.thingDef);
                        product.stackCount = option.amount.RandomInRange;
                        productList.Add(product);
                    }
                }
            }

            // Checking to add Bone and Fat and adding to list
            if (__instance.RaceProps.IsFlesh && __instance.RaceProps.meatDef != null)
            {
                var butcherProperties = ButcherProperties.Get(pawn.def);
                var boneFlag = butcherProperties?.hasBone ?? !__instance.RaceProps.Insect;
                var fatFlag = butcherProperties?.hasFat ?? !__instance.RaceProps.Insect;

                if (boneFlag || fatFlag)
                {
                    var amount = Math.Max(1, (int)(GenMath.RoundRandom(pawn.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) * 0.1f));
                    if (boneFlag)
                    {
                        var bone = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_Bone);
                        bone.stackCount = amount;
                        productList.Add(bone);
                    }
                    if (fatFlag)
                    {
                        var fat = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_Fat);
                        fat.stackCount = amount;
                        productList.Add(fat);
                    }
                }
            }

            return productList;
        }
    }
}
