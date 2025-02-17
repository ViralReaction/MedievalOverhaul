using System.Collections.Generic;
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
            // Store GroundCells to avoid duplicate calls
            List<IntVec3> groundCells = [];
            foreach (IntVec3 cell in CompPowerPlantWater.GroundCells(loc, rot))
            {
                groundCells.Add(cell);
            }
            GenDraw.DrawFieldEdges(groundCells, Color.white, null);

            Color color = this.WaterCellsPresent(loc, rot, Find.CurrentMap) ? Designator_Place.CanPlaceColor.ToOpaque()
                : Designator_Place.CannotPlaceColor.ToOpaque();

            List<IntVec3> waterCells = [];
            foreach (IntVec3 cell in CompPowerPlantWater.WaterCells(loc, rot))
            {
                waterCells.Add(cell);
            }
            GenDraw.DrawFieldEdges(waterCells, color, null);

            // Check overlapping water mills
            bool flag = false;
            CellRect cellRect = CompPowerPlantWater.WaterUseRect(loc, rot);

            // Add existing Watermills
            foreach (Building building in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator))
            {
                PlaceWorker_WaterWheel.waterMills.Add(building);
            }
            foreach (Building building in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(MedievalOverhaulDefOf.DankPyon_WaterMill))
            {
                PlaceWorker_WaterWheel.waterMills.Add(building);
            }

            // Add blueprints and frames for watermills
            foreach (Thing millThing in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint))
            {
                if (millThing.def.entityDefToBuild == ThingDefOf.WatermillGenerator ||
                    millThing.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill)
                {
                    PlaceWorker_WaterWheel.waterMills.Add(millThing);
                }
            }
            foreach (Thing millThing in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame))
            {
                if (millThing.def.entityDefToBuild == ThingDefOf.WatermillGenerator ||
                    millThing.def.entityDefToBuild == MedievalOverhaulDefOf.DankPyon_WaterMill)
                {
                    PlaceWorker_WaterWheel.waterMills.Add(millThing);
                }
            }

            // Process existing WaterMills
            foreach (Thing waterMill in PlaceWorker_WaterWheel.waterMills)
            {
                // Draw field edges for WaterUseCells
                List<IntVec3> waterUseCells = [];
                foreach (IntVec3 cell in CompPowerPlantWater.WaterUseCells(waterMill.Position, waterMill.Rotation))
                {
                    waterUseCells.Add(cell);
                }
                GenDraw.DrawFieldEdges(waterUseCells, new Color(0.2f, 0.2f, 1f), null);

                // Check for overlapping water use areas
                if (cellRect.Overlaps(CompPowerPlantWater.WaterUseRect(waterMill.Position, waterMill.Rotation)))
                {
                    flag = true;
                }
            }
            PlaceWorker_WaterWheel.waterMills.Clear();

            // Adjust color based on overlap
            Color color2 = flag ? new Color(1f, 0.6f, 0f) : Designator_Place.CanPlaceColor.ToOpaque();

            // Draw flashing effect only if needed
            if (!flag || Time.realtimeSinceStartup % 0.4f < 0.2f)
            {
                waterCells.Clear();
                foreach (IntVec3 cell in CompPowerPlantWater.WaterUseCells(loc, rot))
                {
                    waterCells.Add(cell);
                }
                GenDraw.DrawFieldEdges(waterCells, color2, null);
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
