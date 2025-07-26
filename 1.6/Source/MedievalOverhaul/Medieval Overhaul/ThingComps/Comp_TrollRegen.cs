using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class Comp_TrollRegen : ThingComp
    {
        private bool fireFlag = false;
        public CompProperties_TrollRegen Props => (CompProperties_TrollRegen)props;
        public override void CompTick()
        {
            base.CompTick();
            if (parent.HasAttachment(ThingDefOf.Fire))
            {
                fireFlag = true;
            }
            if (parent.IsHashIntervalTick(Props.tickInterval) && !fireFlag)
            {
                if (parent is Pawn pawn && pawn.health != null)
                {
                    List<Hediff_Injury> injuryList = [];
                    List<Hediff> injuryCheck = pawn.health.hediffSet.hediffs;
                    for (int i = 0; i < injuryCheck.Count; i++)
                    {
                        if (injuryCheck[i] is Hediff_Injury injury)
                        {
                            injuryList.Add(injury);
                        }
                    }
                    if (injuryList.Count > 0)
                    {
                        Hediff_Injury hurt = injuryList.RandomElement();
                        hurt.Severity = hurt.Severity - Props.healAmount;
                    }
                }
                LessonAutoActivator.TeachOpportunity(MedievalOverhaulDefOf.DankPyon_Concept_Regeneration, OpportunityType.GoodToKnow);

            }
            if (parent.IsHashIntervalTick(Props.tickRegenBurn) && fireFlag)
            {
                if (!parent.HasAttachment(ThingDefOf.Fire))
                {
                    fireFlag = false;
                }
            }
        }
    }
}