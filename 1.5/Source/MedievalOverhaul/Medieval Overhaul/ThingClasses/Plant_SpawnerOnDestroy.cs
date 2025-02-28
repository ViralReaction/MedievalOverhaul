﻿using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class Plant_SpawnerOnDestroy : Plant
    {
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            if (HarvestableNow)
            {
                SecondaryPlantDropExtension props = SecondaryPlantDropExtension.Get(def);
                if (props != null && props.secondaryDrop != null)
                {
                    if (!props.secondaryNotWhenLeafless || !LeaflessNow)
                    {
                        if (Rand.Chance(props.secondaryDropChance))
                        {
                            Thing droppedThing = ThingMaker.MakeThing(props.secondaryDrop);
                            droppedThing.stackCount = props.secondaryDropAmountRange.RandomInRange;
                            GenPlace.TryPlaceThing(droppedThing, Position, Map, ThingPlaceMode.Near);
                        }
                    }
                }
            }
            Comp_PawnSpawnerOnDestroy comp = this.GetComp<Comp_PawnSpawnerOnDestroy>();
            if (comp != null && (!comp.Props.onlyIfHarvestable || comp.Props.onlyIfHarvestable && this.HarvestableNow))
                comp.PawnSpawnerWorker(by.Map);
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
