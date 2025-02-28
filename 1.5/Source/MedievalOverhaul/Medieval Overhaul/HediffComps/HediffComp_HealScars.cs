using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
	public class HediffComp_HealScars : HediffComp
	{
		private int ticksToHeal;

		public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

		public override void CompPostMake()
		{
			base.CompPostMake();
			ResetTicksToHeal();
		}

		private void ResetTicksToHeal()
		{
			ticksToHeal = Rand.Range(15, 30) * 60000;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToHeal--;
			if (ticksToHeal <= 0)
			{
				TryHealRandomPermanentWound();
				ResetTicksToHeal();
			}
		}

		private void TryHealRandomPermanentWound()
		{
            List<Hediff> permanentHediffs = new List<Hediff>();

            foreach (Hediff hediff in base.Pawn.health.hediffSet.hediffs)
            {
                if (hediff.IsPermanent())
                {
                    permanentHediffs.Add(hediff);
                }
            }
            if (permanentHediffs.Count > 0)
            {
                Hediff result = permanentHediffs.RandomElement();
                HealthUtility.Cure(result);

                if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
                {
                    Messages.Message("MessagePermanentWoundHealed".Translate(parent.LabelCap, base.Pawn.LabelShort, result.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
                }
            }

        }

        public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + ticksToHeal;
		}
	}
}
