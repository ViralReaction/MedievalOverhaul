using RimWorld;

namespace MedievalOverhaul
{
    [DefOf]
    public class StuffabilityDefOf
    {
        public static StuffCategoryDef DankPyon_RawWood;

        static StuffabilityDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(StuffabilityDefOf));
    }
}
