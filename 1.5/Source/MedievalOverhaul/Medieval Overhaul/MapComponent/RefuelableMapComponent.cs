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
                        if (comp != null && comp is CompRefuelableStat)
                        {
                            Register(thing);
                            break;
                        }
                    }
                }
            }

        }
        public void Reset()
        {
            this.refuelableStatThing.Clear();
        }
        public void Register(ThingWithComps thing)
        {
            if (refuelableStatThing.Contains(thing))
            {
                return;
            }
            refuelableStatThing.Add(thing);
        }
        public void Deregister(ThingWithComps thing)
        {
            if (!refuelableStatThing.Contains(thing))
            {
                return;
            }
            refuelableStatThing.Remove(thing);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach (var thing in refuelableStatThing)
            {
                Log.Message(thing);
            }
        }

        public HashSet<Thing> refuelableStatThing = [];
    }
}
