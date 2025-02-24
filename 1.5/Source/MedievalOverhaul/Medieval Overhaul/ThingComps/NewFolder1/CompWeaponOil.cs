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
        public ThingDef targetOilRefillDef;
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
                Scribe_Defs.Look(ref targetOilRefillDef, "targetOilRefillDef");
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
            targetOilRefillDef = oilRefillDef;
            oilCharges = charges; // Reset duration
            maxCharges = charges;
        }

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
