using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

    public static class Harmony_CombatExtended
    {
        private static Type Verb_MeleeAttackCE_Patch
        {
            get
            {
                return AccessTools.TypeByName("CombatExtended.Verb_MeleeAttackCE");
            }
        }
        [HarmonyPatch]
        public static class Verb_MeleeAttackCE_ApplyMeleeDamageToTarget
        {
            public static bool Prepare()
            {
                return Verb_MeleeAttackCE_Patch != null;
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("CombatExtended.Verb_MeleeAttackCE:ApplyMeleeDamageToTarget");
            }

            public static void Postfix(ref DamageWorker.DamageResult __result, Verb_MeleeAttack __instance, LocalTargetInfo target)
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
                            foreach ( Hediff hediff in list)
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
}
