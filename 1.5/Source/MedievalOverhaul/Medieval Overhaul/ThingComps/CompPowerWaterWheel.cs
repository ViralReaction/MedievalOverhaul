using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class CompPowerWaterWheel : CompPowerPlantWater
    {
        public override float DesiredPowerOutput
        {
            get
            {
                if (this.cacheDirty)
                {
                    this.RebuildThingCache();
                }
                if (!this.waterUsable)
                {
                    return 0f;
                }
                if (this.waterDoubleUsed)
                {
                    return base.DesiredPowerOutput * 0.3f;
                }
                return -base.Props.PowerConsumption;
            }
        }

        public float WheelProcessSpeed
        {
            get
            {
                if (!this.waterUsable)
                {
                    return 0f;
                }
                if (this.waterDoubleUsed)
                {
                    return 0.3f;
                }
                return 1f;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (PowerOn)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    StartSustainerPoweredIfInactive();
                });
            }
            UpdateOverlays();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
            if (base.Props.PowerConsumption < 0f && !parent.IsBrokenDown() && FlickUtility.WantsToBeOn(parent))
            {
                base.PowerOn = true;
            }
            this.spinPosition = Rand.Range(0f, 15f);
            this.RebuildThingCache();
            this.RebuildMapCache(this.parent.Map);
        }
        public override void PostDeSpawn(Map map)
        {
            this.RebuildMapCache(map);
        }
        private void RebuildThingCache()
        {
            this.waterUsable = true;
            foreach (IntVec3 c in this.WaterCells())
            {
                if (c.InBounds(this.parent.Map) && !this.parent.Map.terrainGrid.TerrainAt(c).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
                {
                    this.waterUsable = false;
                    break;
                }
            }
            this.waterDoubleUsed = false;
            List<Building> waterGenerator = [];

            if (this.parent.Map.listerThings.listsByDef.TryGetValue(ThingDefOf.WatermillGenerator, out List<Thing> generatorThings))
            {
                foreach (Thing thing in generatorThings)
                {
                    if (thing is Building building)
                    {
                        waterGenerator.Add(building);
                    }
                }
            }
            foreach (IntVec3 c2 in this.WaterUseCells())
            {
                if (c2.InBounds(this.parent.Map))
                {
                    foreach (Building building in waterGenerator)
                    {
                        if (building != this.parent && building.GetComp<CompPowerPlantWater>().WaterUseRect().Contains(c2))
                        {
                            this.waterDoubleUsed = true;
                            break;
                        }
                    }
                }
            }
            List<Building> waterWheel = [];

            if (this.parent.Map.listerThings.listsByDef.TryGetValue(MedievalOverhaulDefOf.DankPyon_WaterMill, out List<Thing> wheelThings))
            {
                foreach (Thing thing in wheelThings)
                {
                    if (thing is Building building)
                    {
                        waterWheel.Add(building);
                    }
                }
            }
            foreach (IntVec3 c2 in this.WaterUseCells())
            {
                if (c2.InBounds(this.parent.Map))
                {
                    foreach (Building building in waterWheel)
                    {
                        if (building != this.parent && building.GetComp<CompPowerWaterWheel>().WaterUseRect().Contains(c2))
                        {
                            this.waterDoubleUsed = true;
                            break;
                        }
                    }
                }
            }
            if (!this.waterUsable)
            {
                this.spinRate = 0f;
                return;
            }
            Vector3 vector = Vector3.zero;
            foreach (IntVec3 intVec in this.WaterCells())
            {
                vector += this.parent.Map.waterInfo.GetWaterMovement(intVec.ToVector3Shifted());
            }
            this.spinRate = Mathf.Sign(Vector3.Dot(vector, this.parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell.ToVector3()));
            this.spinRate *= Rand.RangeSeeded(0.9f, 1.1f, this.parent.thingIDNumber * 60509 + 33151);
            if (this.waterDoubleUsed)
            {
                this.spinRate *= 0.5f;
            }
            this.cacheDirty = false;
        }

        public void RebuildMapCache(Map map)
        {
            var listThing = map.listerThings.AllThings;
            for (int i = 0; i < listThing.Count; i++)
            {
                if (listThing[i] is Building building)
                {
                    var buildingDef = building.def;
                    if (buildingDef == ThingDefOf.WatermillGenerator)
                    {
                        building.TryGetComp<CompPowerPlantWater>().ClearCache();
                    }
                    if (buildingDef == MedievalOverhaulDefOf.DankPyon_WaterMill)
                    {
                        building.TryGetComp<CompPowerWaterWheel>().ClearCache();
                    }
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            if (this.waterUsable && this.waterDoubleUsed)
            {
                return "Watermill_WaterUsedTwice".Translate();

            }
            return null;
        }

        public override void CompTick()
        {
            this.UpdateDesiredPowerOutput();
            this.spinPosition = (this.spinPosition + 0.006666667f * this.spinRate + 6.2831855f) % 6.2831855f;
        }


        //public static readonly Material BladesMat = MaterialPool.MatFrom("Things/Building/Power/WatermillGenerator/WatermillGeneratorBlades");
    }
}
