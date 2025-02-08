using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using Verse;

namespace MedieavlOverhaul.Patches
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Patch_VerbMeleeAttack_TryCastShot
    {
        public static void Postfix(bool __result, Verb_MeleeAttack __instance)
        {
            if (!__result) return;

            Pawn attacker = __instance.CasterPawn;
            if (attacker?.equipment?.Primary is ThingWithComps weapon)
            {
                CompPoisonWeapon poisonComp = weapon.TryGetComp<CompPoisonWeapon>();
                if (poisonComp != null && poisonComp.poisonCharges > 0)
                {
                    if (__instance.currentTarget.Thing is Pawn target)
                    {
                        target.health.AddHediff(poisonComp.hediffDef);
                        poisonComp.UseCharge();
                    }
                }
            }
        }
    }
}