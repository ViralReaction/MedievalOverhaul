using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class CompStoreFuelThing : ThingComp, ICompFuelHandler
    {
        public ThingDef fuelUsed;
        private ThingFilter allowedFuelFilter;
        public ThingFilter filter;

        public CompRefuelable compRefuelable;

        public ThingFilter AllowedFuelFilter => this.allowedFuelFilter;

        private static HashSet<string> loggedDefs = new();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (Utility.LWMFuelFilterIsEnabled)
                return;
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.allowedFuelFilter != null)
                return;
            compRefuelable = this.parent.GetComp<CompRefuelable>();
            if (compRefuelable == null)
            {
                string defName = this.parent.def.defName;
                if (!loggedDefs.Contains(defName))
                {
                    Log.Error($"Medieval Overhaul: {defName} is missing CompRefuelable");
                    loggedDefs.Add(defName);
                }
                return;
            }
            this.allowedFuelFilter = new ThingFilter();
            this.allowedFuelFilter.CopyAllowancesFrom(compRefuelable.Props.fuelFilter);
        }

        public override void PostExposeData()
        {
            if (Utility.LWMFuelFilterIsEnabled)
                return;
            base.PostExposeData();
            Scribe_Defs.Look<ThingDef>(ref this.fuelUsed, "fuelUsed");
            Scribe_Deep.Look<ThingFilter>(ref this.allowedFuelFilter, "allowedFuelFilter");
        }

        public override string CompInspectStringExtra() => Utility.LWMFuelFilterIsEnabled || compRefuelable == null ? (string)null : (string)(!compRefuelable.HasFuel || this.fuelUsed == null ? (TaggedString)(string)null : "ESCP_Tools_FuelExtension_CurrentFuel".Translate((NamedArgument)this.fuelUsed.label));

    }
}
