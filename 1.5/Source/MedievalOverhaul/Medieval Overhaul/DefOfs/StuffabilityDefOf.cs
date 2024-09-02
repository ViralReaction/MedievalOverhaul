using RimWorld;

namespace MedievalOverhaul
{
    [DefOf]
    public static class StuffabilityDefOf
    {
        #pragma warning disable CS0649
        public static StuffCategoryDef DankPyon_RawWood;
        #pragma warning restore CS0649

        static StuffabilityDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(StuffabilityDefOf));
    }
}
