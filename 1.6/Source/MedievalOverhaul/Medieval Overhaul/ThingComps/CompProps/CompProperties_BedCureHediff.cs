using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_BedCureHediff : CompProperties
    {
        public HediffDef cureHediff;
        public int intervalTick;
        public float cureSeverity = 0.05f;

        public CompProperties_BedCureHediff() => this.compClass = typeof(Comp_BedCureHediff);
    }
}
