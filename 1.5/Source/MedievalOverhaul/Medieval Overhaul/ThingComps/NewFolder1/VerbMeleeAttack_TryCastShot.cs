using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class VerbMeleeAttack_TryCastShot
    {
        public static void Postfix(bool __result, Verb_MeleeAttack __instance)
        {
            if (!__result) return;

            Pawn attacker = __instance.CasterPawn;
            if (attacker?.equipment?.Primary is ThingWithComps weapon)
            {
                CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
                if (oilComp != null && oilComp.oilCharges > 0)
                {
                    if (__instance.currentTarget.Thing is Pawn target)
                    {
                        target.health.AddHediff(oilComp.hediffDef);
                        oilComp.UseCharge();
                    }
                }
            }
        }
    }
}