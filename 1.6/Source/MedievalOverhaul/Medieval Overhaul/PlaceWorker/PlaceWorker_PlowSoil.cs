using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class PlaceWorker_PlowSoil : PlaceWorker
    {
        private static float terrainFertilityMax => MedievalOverhaulDefOf.DankPyon_PlowedSoil.fertility;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            TerrainDef terrain = map.terrainGrid.TerrainAt(loc);
            if (terrain == null || terrain.defName == "DankPyon_PlowedSoil" || !(terrain.fertility <= terrainFertilityMax)) return AcceptanceReport.WasRejected;
            List<TerrainAffordanceDef> list = terrain.affordances;
            if (list.Contains(MedievalOverhaulDefOf.GrowSoil) && list.Contains(TerrainAffordanceDefOf.Medium))
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasRejected;
            
        }
    }
}
