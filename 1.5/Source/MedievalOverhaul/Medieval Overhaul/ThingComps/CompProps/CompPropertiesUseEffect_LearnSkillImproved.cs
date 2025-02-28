using RimWorld;

namespace MedievalOverhaul
{
    internal class CompPropertiesUseEffect_LearnSkillImproved : CompProperties_UseEffect
    {
        public string skillDefName = null;
        public float xpAmount = 0;

        public CompPropertiesUseEffect_LearnSkillImproved() => this.compClass = typeof(CompUseEffect_LearnSkillImproved);
    }
}