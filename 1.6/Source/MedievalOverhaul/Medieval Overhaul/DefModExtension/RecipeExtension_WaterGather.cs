using Verse;

namespace MedievalOverhaul
{
    class RecipeExtension_WaterGather : DefModExtension
    {
        #pragma warning disable
        public ThingDef thingDef;
        #pragma warning restore
        public int productCount = 1;
        public static RecipeExtension_WaterGather Get(Def def)
        {
            return def.GetModExtension<RecipeExtension_WaterGather>();
        }
    }
}
