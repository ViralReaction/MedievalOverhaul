using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class DamageWorker_AlpSleep : DamageWorker
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();
            if (victim is Pawn pawn && pawn.needs.rest != null)
            {
                var hediffSet = pawn.health.hediffSet;
                bool canSleep = true;
                for (int i = 0; i < Utility.StimulantDrugList.stimulant.Count; i++)
                {
                    if (hediffSet.HasHediff(Utility.StimulantDrugList.stimulant[i]))
                    {
                        canSleep = false;
                        break;
                    }
                }
                float restDamage = dinfo.Amount / 100f;
                var needRest = pawn.needs.rest;
                needRest.CurLevel = Mathf.Clamp01(needRest.CurLevel - restDamage * pawn.GetStatValueForPawn(MedievalOverhaulDefOf.RestFallRateFactor, pawn));
                float applyChance = 0.5f;               
                if (canSleep && needRest.curLevelInt == 0f && dinfo.Def.hediff != null && Rand.Chance(applyChance))
                {
                    Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn, null);
                    hediff.Severity = 1f;
                    pawn.health.AddHediff(hediff, null, new DamageInfo?(dinfo), null);
                }
            }
            return result;
        }
    }
}