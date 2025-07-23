using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    [DefOf]
    public class MedievalOverhaulThingDefOf
    {
        public static ThingDef DankPyon_RawWood;
        public static ThingDef DankPyon_RawDarkWood;

        static MedievalOverhaulThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MedievalOverhaulThingDefOf));
    }
}
