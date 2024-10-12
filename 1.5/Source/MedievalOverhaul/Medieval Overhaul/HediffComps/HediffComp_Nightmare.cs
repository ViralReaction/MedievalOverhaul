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