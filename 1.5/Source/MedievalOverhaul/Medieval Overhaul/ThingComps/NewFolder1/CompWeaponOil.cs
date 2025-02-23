using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompWeaponOil : ThingComp
    {
        public int oilCharges = 20; // 1 in-game day
        public int maxCharges = 20;
        public HediffDef hediffDef;
        public ThingDef oilRefillDef;
        public string oilType;
        public bool autoRefuel = false; // New setting
        public override void PostExposeData()
        {
            base.PostExposeData();

            Log.Message($"[CompWeaponOil] PostExposeData called for {parent?.LabelCap ?? "Unknown Weapon"} - Mode: {Scribe.mode}");

            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref oilCharges, "oilCharges", 0);
                Scribe_Values.Look(ref maxCharges, "maxCharges", 0);
                Scribe_Defs.Look(ref hediffDef, "hediffDef");
                Scribe_Defs.Look(ref oilRefillDef, "oilRefillDef");
                Scribe_Values.Look(ref oilType, "oilType");
                Scribe_Values.Look(ref autoRefuel, "autoRefuel", false, false);
            }

            // Debugging to confirm values were loaded
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Message($"[CompWeaponOil] Loaded Data -> oilCharges: {oilCharges}, maxCharges: {maxCharges}, oilType: {oilType}, autoRefuel: {autoRefuel}");
            }
        }
       

        public void RefreshOil(int charges, string oilName, HediffDef oilHediff, ThingDef refillDef)
        {
            oilType = oilName;
            hediffDef = oilHediff;
            oilRefillDef = refillDef;
            oilCharges = charges; // Reset duration
        }

        //public override void PostSpawnSetup(bool respawningAfterLoad)
        //{
        //    OilWeaponTrackerComponent comp = OiledWeaponsManager.GetComp();
        //    comp.RegisterOilWeapon(this.parent);
        //}

        //public override void PostDeSpawn(Map map)
        //{
        //    base.PostDeSpawn(map);
        //    OilWeaponTrackerComponent comp = OiledWeaponsManager.GetComp();
        //    comp.DeregisterOilWeapon(this.parent);
        //}

        public void UseCharge()
        {
            oilCharges--;
        }

        public override string CompInspectStringExtra()
        {
            return "DankPyon_OilWeaponRemaining".Translate(oilCharges);
        }

        public override string TransformLabel(string baseLabel)
        {
            if (!string.IsNullOrEmpty(oilType) && oilCharges > 0)
            {
                return $"{baseLabel} ({oilType})"; // Appends oil type to weapon label
            }
            return baseLabel; // If no oil, return normal label
        }
    }

}
