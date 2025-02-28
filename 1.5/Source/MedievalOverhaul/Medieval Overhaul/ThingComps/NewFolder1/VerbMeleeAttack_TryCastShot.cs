using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    //[HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    //public static class VerbMeleeAttack_TryCastShot
    //{
    //    public static void Postfix(bool __result, Verb_MeleeAttack __instance)
    //    {
    //        if (!__result) return;

    //        Pawn attacker = __instance.CasterPawn;
    //        if (attacker?.equipment?.Primary is ThingWithComps weapon)
    //        {
    //            CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
    //            if (oilComp != null && oilComp.oilCharges > 0)
    //            {
    //                if (__instance.currentTarget.Thing is Pawn target)
    //                {
    //                    target.health.AddHediff(oilComp.hediffDef);
    //                    oilComp.UseCharge();
    //                }
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class VerbMeleeAttack_ApplyMeleeDamageToTarget
    {
        public static void Postfix(DamageWorker.DamageResult __result, Verb_MeleeAttackDamage __instance)
        {
            if (__result.totalDamageDealt == 0) return;

            Pawn attacker = __instance.CasterPawn;
            if (attacker?.equipment?.Primary is ThingWithComps weapon)
            {
                CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
                if (oilComp != null && oilComp.oilCharges > 0)
                {
                    if (__instance.currentTarget.Thing is Pawn victim)
                    {
                        var list = __result.hediffs;
                        bool lethalFlag = false;
                        foreach (Hediff hediff in list)
                        {
                            if (hediff.IsLethal && hediff.Severity > 0)
                            {
                                
                                lethalFlag = true;
                                break;
                            }
                        }
                        if (lethalFlag == true)
                        {
                            victim.health.AddHediff(oilComp.hediffDef);
                            oilComp.UseCharge();
                        }
                    }
                }
            }
        }
    }
}