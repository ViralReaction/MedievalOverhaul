using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class HediffComp_LindwurmAcid : HediffComp
    {
        private List<Apparel> apparelList;
        private int apparelDamageAmount;
        public HediffCompProperties_LindwurmAcid Props
        {
            get
            {
                return (HediffCompProperties_LindwurmAcid)this.props;
            }
        }
        public override bool CompShouldRemove
        {
            get
            {
                Pawn pawn = this.Pawn;
                if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_LindwurmAcidImmune))
                {
                    return true;
                }
                return false;
            }
        }

        public override void CompPostMake()
        {
            apparelDamageAmount = Props.apparelDamagePerInterval;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                Pawn pawn = Pawn;
                if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_LindwurmAcidImmune)) return;
                apparelList = pawn.apparel?.WornApparel;
                if (apparelList == null || apparelList.Count == 0) return;
                List<Thing> apparelListCopy = new List<Thing>(apparelList.Count);
                for (int i = 0; i < apparelList.Count; i++)
                {
                    apparelListCopy.Add(apparelList[i]);
                }
                for (int i = 0; i < apparelListCopy.Count; i++)
                {
                    Thing apparel = apparelListCopy[i];
                    if (apparel == null) continue;
                    if (apparel.HitPoints <= apparelDamageAmount)
                    {
                        apparel.HitPoints = 0;
                        apparel.Destroy();
                    }
                    else
                    {
                        apparel.HitPoints -= apparelDamageAmount;
                    }
                }
            }

        }
    }
}