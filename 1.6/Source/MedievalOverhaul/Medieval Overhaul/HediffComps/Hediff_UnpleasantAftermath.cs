using Verse;

namespace MedievalOverhaul
{
	public class Hediff_UnpleasantAftermath : HediffWithComps
	{
		public override bool Visible
		{
			get
			{
				if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_UnpleasantAftermath))
				{
					return false;
				}
				return base.Visible;
			}
		}
	}
}
