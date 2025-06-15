using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Plant_SecondaryDrop : Plant
    {
        #region Fields

        private ThingDef secondaryDrop;
        private IntRange secondaryDropAmountRange = new IntRange(1, 1);
        private float secondaryDropChance = 1f;
        private bool secondaryNotWhenLeafless;
        private bool secondaryAffectedBySkill;

        #endregion

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            SecondaryPlantDropExtension props = SecondaryPlantDropExtension.Get(def);
            if (props == null)
            {
                Log.Error($"{this.def} is missing required SecondaryPlantDropExtension");
                return;
            }
            secondaryDrop = props.secondaryDrop ?? def.plant.harvestedThingDef;
            secondaryDropAmountRange = props.secondaryDropAmountRange;
            secondaryDropChance = props.secondaryDropChance;
            secondaryNotWhenLeafless = props.secondaryNotWhenLeafless;
            secondaryAffectedBySkill = props.affectedBySkill;
        }

        public override void PlantCollected(Pawn pawn, PlantDestructionMode plantDestructionMode)
        {
            if (HarvestableNow)
            {
                if ((!secondaryNotWhenLeafless || !LeaflessNow) && Rand.Chance(secondaryDropChance))
                {
                    Thing thing = ThingMaker.MakeThing(secondaryDrop);
                    thing.stackCount = SecondaryYieldNow(pawn);
                    GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near, null, null, default(Rot4));
                }
            }
            base.PlantCollected(pawn, plantDestructionMode);
        }

        private int SecondaryYieldNow(Pawn pawn)
        {
            if (!CanYieldNow())
            {
                return 0;
            }
            if (!secondaryAffectedBySkill) return secondaryDropAmountRange.RandomInRange;
            float harvestYield = secondaryDropAmountRange.max;
            StatDef stat = secondaryDrop.IsDrug ? StatDefOf.DrugHarvestYield : StatDefOf.PlantHarvestYield;
            float statValue = pawn.GetStatValue(stat);
            float num = Mathf.InverseLerp(def.plant.harvestMinGrowth, 1f, growthInt);
            num = 0.5f + num * 0.5f;
            harvestYield *= num;
            harvestYield *= Mathf.Lerp(0.5f, 1f, HitPoints / (float)MaxHitPoints);
            if (def.plant.harvestYieldAffectedByDifficulty)
            {
                harvestYield *= Find.Storyteller.difficulty.cropYieldFactor;
            }
            if (statValue > 1f)
            {
                harvestYield = GenMath.RoundRandom(harvestYield * statValue);
            }
            return GenMath.RoundRandom(harvestYield);
        }

    }
}
