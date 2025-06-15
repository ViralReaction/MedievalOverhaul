using Verse;
namespace MedievalOverhaul
{
    public class SecondaryPlantDropExtension : DefModExtension
    {
        public ThingDef secondaryDrop = null;
        public IntRange secondaryDropAmountRange = new IntRange(1, 1);
        public float secondaryDropChance = 1f;
        public bool secondaryNotWhenLeafless = false;
        public bool affectedBySkill = false;
        public static SecondaryPlantDropExtension Get(Def def)
        {
            return def.GetModExtension<SecondaryPlantDropExtension>();
        }
    }
}
