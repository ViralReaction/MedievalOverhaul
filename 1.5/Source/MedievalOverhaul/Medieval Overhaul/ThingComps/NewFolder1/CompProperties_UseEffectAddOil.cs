using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_UseEffectAddOil : CompProperties_UseEffect
    {
        public CompProperties_UseEffectAddOil()
        {
            this.compClass = typeof(CompUseEffect_AddOil);
        }

        public HediffDef hediffDef;
        public int maxCharges;
        public string oilType;

    }
}
