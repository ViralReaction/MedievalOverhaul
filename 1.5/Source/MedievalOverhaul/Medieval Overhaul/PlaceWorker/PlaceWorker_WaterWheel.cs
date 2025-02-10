using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace MedievalOverhaul
{
    public class PlaceWorker_WaterWheel : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in CompPowerWaterWheel.GroundCells(loc, rot))
            {
                if (!map.terrainGrid.TerrainAt(c).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                {
                    return new AcceptanceReport("TerrainCannotSupport_TerrainAffordance".Translate(checkingDef, TerrainAffordanceDefOf.Heavy));
                }
            }
            if (!this.WaterCellsPresent(loc, rot, map))
            {
                return new AcceptanceReport("MustBeOnMovingWater".Translate());
            }
            return true;
        }

        private bool WaterCellsPresent(IntVec3 loc, Rot4 rot, Map map)
        {
            foreach (IntVec3 c in CompPowerWaterWheel.WaterCells(loc, rot))
            {
                if (!c.InBounds(map) || !map.terrainGrid.TerrainAt(c).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
                {
                    return false;
                }
            }
            return true;
        }
        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(CompPowerPlantWater.GroundCells(loc, rot).ToList<IntVec3>(), Color.white, null);
            Color color = this.WaterCellsPresent(loc, rot, Find.CurrentMap) ? Designator_Place.CanPlaceColor.ToOpaque() : Designator_Place.CannotPlaceColor.ToOpaque();
            GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterCells(loc, rot).ToList<IntVec3>(), color, null);
            bool flag = false;
            CellRect cellRect = CompPowerPlantWater.WaterUseRect(loc, rot);
            PlaceWorker_WaterWheel.waterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator).Cast<Thing>());
            PlaceWorker_WaterWheel.waterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(MedievalOverhaulDefOf.DankPyon_WaterMill).Cast<Thing>());
            PlaceWorker_WaterWheel.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                                                               where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                               select t);
            PlaceWorker_WaterWheel.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                                                               where t.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill
                                                               select t);
            PlaceWorker_WaterWheel.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
                                                               where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                               select t);
            PlaceWorker_WaterWheel.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
                                                               where t.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill
                                                               select t);
            foreach (Thing waterMill in PlaceWorker_WaterWheel.waterMills)
            {
                GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(waterMill.Position, waterMill.Rotation).ToList<IntVec3>(), new Color(0.2f, 0.2f, 1f), null);
                if (cellRect.Overlaps(CompPowerPlantWater.WaterUseRect(waterMill.Position, waterMill.Rotation)))
                {
                    flag = true;
                }
            }
            PlaceWorker_WaterWheel.waterMills.Clear();
            Color color2 = flag ? new Color(1f, 0.6f, 0f) : Designator_Place.CanPlaceColor.ToOpaque();
            if (!flag || Time.realtimeSinceStartup % 0.4f < 0.2f)
            {
                GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(loc, rot).ToList<IntVec3>(), color2, null);
            }
        }

        public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
        {
            yield return TerrainAffordanceDefOf.Heavy;
            yield return TerrainAffordanceDefOf.MovingFluid;
            yield break;
        }

        protected static List<Thing> waterMills = new List<Thing>();
    }
}
