using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class DamageWorker_AlpSleep : DamageWorker
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();
            Pawn pawn = victim as Pawn;
            if (pawn != null && pawn.needs.rest != null)
            {
                float sleepDamage = dinfo.Amount / 100f;
                float newRestLevel = pawn.needs.rest.curLevelInt - sleepDamage;
                pawn.needs.rest.curLevelInt = newRestLevel < 0 ? 0 : newRestLevel;
                if (pawn.needs.rest.curLevelInt < 0.3f)
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
