using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class PlaceWorker_WaterGather : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                if (!c.InBounds(map))
                {
                    return new AcceptanceReport("DankPyon_InBound".Translate());
                }
                var terrain = map.terrainGrid.TerrainAt(c);
                if (!terrain.IsWater && terrain.defName != "Ice")
                {
                    return new AcceptanceReport("DankPyon_NeedsWater".Translate());
                }
            }
            return AcceptanceReport.WasAccepted;
        }

    }
}
