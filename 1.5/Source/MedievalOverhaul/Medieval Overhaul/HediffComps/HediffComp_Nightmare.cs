using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class HediffComp_Nightmare : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            Pawn pawn = this.Pawn;
            var hediffSet = pawn.health.hediffSet;
            foreach (var hediffDef in Utility.StimulantDrugList.stimulant)
            {
                Log.Message(hediffDef);

                if (hediffSet.HasHediff(hediffDef))
                {
                    return;
                }
            }
            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                if (part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
                {
                    pawn.TakeDamage(new DamageInfo(MedievalOverhaulDefOf.DankPyon_AlpNightmare, 1f, 300f, -1, null, part));
                    break;
                }
            }
            
        }
    }
}