using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class RecipeWorkerCounter_GatherWater : RecipeWorkerCounter
    {
        private ThingDef _cachedThingDef;
        public override int CountProducts(Bill_Production bill)
        {
            var waterExtension = this.recipe.GetModExtension<RecipeExtension_WaterGather>();
            ThingDefCountClass thingDefCountClass = this.recipe.products[0];
            if (_cachedThingDef == null)
            {
                if (waterExtension != null && waterExtension.thingDef != null && bill.billStack?.billGiver is Thing billGiver && billGiver.Map.terrainGrid.TerrainAt(billGiver.Position).defName == "Ice")
                {

                    _cachedThingDef = waterExtension.thingDef;

                }
                else
                {
                    _cachedThingDef = thingDefCountClass.thingDef;
                }
            }
            ThingDef thingDef = _cachedThingDef;

            if (thingDef.CountAsResource && bill.GetIncludeSlotGroup() == null && !bill.limitToAllowedStuff)
            {
                return bill.Map.resourceCounter.GetCount(thingDef) + this.GetCarriedCount(bill, thingDef);
            }
            int num = 0;
            ISlotGroup includeSlotGroup = bill.GetIncludeSlotGroup();
            if (includeSlotGroup == null)
            {
                num = this.CountValidThings(bill.Map.listerThings.ThingsOfDef(thingDef), bill, thingDef);
                if (thingDef.Minifiable)
                {
                    List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
                    for (int i = 0; i < list.Count; i++)
                    {
                        MinifiedThing minifiedThing = (MinifiedThing)list[i];
                        if (this.CountValidThing(minifiedThing.InnerThing, bill, thingDef))
                        {
                            num += minifiedThing.stackCount * minifiedThing.InnerThing.stackCount;
                        }
                    }
                }
                num += this.GetCarriedCount(bill, thingDef);
            }
            else
            {
                foreach (Thing outerThing in includeSlotGroup.HeldThings)
                {
                    Thing innerIfMinified = outerThing.GetInnerIfMinified();
                    if (this.CountValidThing(innerIfMinified, bill, thingDef))
                    {
                        num += innerIfMinified.stackCount;
                    }
                }
            }
            return num;
        }

    }
}
