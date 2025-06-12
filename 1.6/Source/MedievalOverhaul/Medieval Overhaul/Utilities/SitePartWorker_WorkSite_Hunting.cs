using RimWorld;
using System.Collections.Generic;
using Verse;
using RimWorld.Planet;

namespace MedievalOverhaul
{
    public class SitePartWorker_WorkSite_Hunting : SitePartWorker_WorkSite
    {
        public override IEnumerable<PreceptDef> DisallowedPrecepts
        {
            get
            {
                foreach (PreceptDef p in DefDatabase<PreceptDef>.AllDefs)
                {
                    if (p.disallowHuntingCamps)
                    {
                        yield return p;
                    }
                }

            }
        }

        public override PawnGroupKindDef WorkerGroupKind => PawnGroupKindDefOf.Hunters;

        public override bool CanSpawnOn(PlanetTile tile)
        {
            if (Find.WorldGrid[tile].AnimalDensity > BiomeDefOf.Desert.animalDensity)
            {
                return base.CanSpawnOn(tile);
            }
            return false;
        }

        public override IEnumerable<SitePartWorker_WorkSite.CampLootThingStruct> LootThings(PlanetTile tile)
        {
            List<ThingDef> enumerable = new List<ThingDef>();

            foreach (PawnKindDef pawnKindDef in Find.WorldGrid[tile].biome.AllWildAnimals)
            {
                if (pawnKindDef?.RaceProps?.leatherDef is not null)
                {
                    enumerable.Add(pawnKindDef.RaceProps.leatherDef);
                }
            }
            int count = 0;
            foreach (ThingDef _ in enumerable)
            {
                count++;
            }
            float leatherWeight = 1f / (float)count;
            foreach (ThingDef thing in enumerable)
            {
                if (thing is not null)
                {
                    if (HideUtility.IsHide(thing))
                    {
                        ThingDefCountClass thingDefCountClass = thing.butcherProducts[0];
                        ThingDef butcherDef = thingDefCountClass.thingDef;
                        yield return new SitePartWorker_WorkSite.CampLootThingStruct
                        {
                            thing = butcherDef,
                            thing2 = ThingDefOf.Pemmican,
                            weight = leatherWeight
                        };
                    }
                    else
                    {
                        yield return new SitePartWorker_WorkSite.CampLootThingStruct
                        {
                            thing = thing,
                            thing2 = ThingDefOf.Pemmican,
                            weight = leatherWeight
                        };
                    }
                }
                else
                {
                    yield return new SitePartWorker_WorkSite.CampLootThingStruct
                    {
                        thing = ThingDefOf.Pemmican,
                        weight = leatherWeight
                    };
                }

                
            }
            yield break;
        }
    }
}
