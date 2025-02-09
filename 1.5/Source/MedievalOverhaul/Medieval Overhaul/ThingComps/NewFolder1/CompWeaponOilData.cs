using Verse;

namespace MedievalOverhaul
{
    public class CompWeaponOilData : IExposable
    {
        public int oilCharges;
        public int maxCharges;
        public HediffDef hediffDef;
        public ThingDef oilRefillDef;
        public string oilType;
        public bool autoRefuel;

        // ✅ Explicit public parameterless constructor (needed for Scribe to deserialize)
        public CompWeaponOilData() { }

        // ✅ Constructor that updates data from an existing CompWeaponOil
        public CompWeaponOilData(CompWeaponOil comp)
        {
            UpdateFromComp(comp);
        }

        public void UpdateFromComp(CompWeaponOil comp)
        {
            oilCharges = comp.oilCharges;
            maxCharges = comp.maxCharges;
            hediffDef = comp.hediffDef;
            oilRefillDef = comp.oilRefillDef;
            oilType = comp.oilType;
            autoRefuel = comp.autoRefuel;
        }

        public void ApplyTo(CompWeaponOil comp)
        {
            comp.oilCharges = oilCharges;
            comp.maxCharges = maxCharges;
            comp.hediffDef = hediffDef;
            comp.oilRefillDef = oilRefillDef;
            comp.oilType = oilType;
            comp.autoRefuel = autoRefuel;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref oilCharges, "oilCharges", 0);
            Scribe_Values.Look(ref maxCharges, "maxCharges", 0);
            Scribe_Defs.Look(ref hediffDef, "hediffDef");
            Scribe_Defs.Look(ref oilRefillDef, "oilRefillDef");
            Scribe_Values.Look(ref oilType, "oilType");
            Scribe_Values.Look(ref autoRefuel, "autoRefuel", false, false);
        }
    }
}