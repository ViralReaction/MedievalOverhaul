using RimWorld;

namespace MedievalOverhaul
{
    public class StorytellerCompProperties_MineshaftInfestation : StorytellerCompProperties
    {
        public float baseMtbDaysPerDrill;

        public StorytellerCompProperties_MineshaftInfestation()
        {
            compClass = typeof(StorytellerComp_MineshaftInfestation);
        }
    }
}