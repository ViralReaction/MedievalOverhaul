using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class RefuelableMapComponent : MapComponent
    {
        public RefuelableMapComponent(Map map) : base(map)
        {
        
        }
        public override void MapGenerated()
        {
            base.MapGenerated();
            GetRefuelableMap();
        }
        public override void MapRemoved()
        {
            base.MapRemoved();

        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            GetRefuelableMap();
        }

        public void GetRefuelableMap()
        {
            foreach (var thing in this.map.listerBuildings.allBuildingsColonist)
            {
                if (thing.comps != null)
                {
                    foreach (var comp in thing.comps)
                    {
                        if (comp != null)
                        {
                            if (comp is CompRefuelableStat)
                            {
                                RegisterRefuelStat(thing);
                            }
                            if (comp is CompRefuelableCustom)
                            {
                                RegisterRefuel(thing);
                            }
                        }

                    }
                }
            }
        }
        public void Reset()
        {
            this.refuelableStatThing.Clear();
        }
        public void RegisterRefuelStat(ThingWithComps thing)
        {
            if (refuelableStatThing.Contains(thing))
            {
                return;
            }
            refuelableStatThing.Add(thing);
        }
        public void DeregisterRefuelStat(ThingWithComps thing)
        {
            if (!refuelableStatThing.Contains(thing))
            {
                return;
            }
            refuelableStatThing.Remove(thing);
        }

        public void RegisterRefuel(ThingWithComps thing)
        {
            if (refuelableCustomThing.Contains(thing))
            {
                return;
            }
            refuelableCustomThing.Add(thing);
        }
        public void DeregisterRefuel(ThingWithComps thing)
        {
            if (!refuelableCustomThing.Contains(thing))
            {
                return;
            }
            refuelableCustomThing.Remove(thing);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
        }

        public HashSet<Thing> refuelableStatThing = [];
        public HashSet<Thing> refuelableCustomThing = [];
    }
}
