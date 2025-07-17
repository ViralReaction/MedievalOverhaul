using Verse;
namespace MedievalOverhaul
{
    public class ScattererValidator_NestTemp : ScattererValidator
    {
        public override bool Allows(IntVec3 c, Map map)
        {
            return map.TileInfo.temperature is >= -10f and <= 40f;
        }

    }
}
