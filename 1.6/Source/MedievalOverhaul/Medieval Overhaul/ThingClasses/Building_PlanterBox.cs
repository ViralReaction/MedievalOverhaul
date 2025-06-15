using System;
using Verse;
using RimWorld;


namespace MedievalOverhaul
{
    public class Building_PlanterBox : Building_PlantGrower
    {

        // Completely Override Original so don't even bother checking for power
        public override void TickRare()
        {
        }

        public override string GetInspectString()
        {
            if (!Spawned)
            {
                throw new Exception("ShouldNeverBeHere");
            }
            bool canGrow = PlantUtility.GrowthSeasonNow(Position, Map, plantDefToGrow);
            return canGrow ? "GrowSeasonHereNow".Translate() : "CannotGrowBadSeasonTemperature".Translate();
        }
    }
}
