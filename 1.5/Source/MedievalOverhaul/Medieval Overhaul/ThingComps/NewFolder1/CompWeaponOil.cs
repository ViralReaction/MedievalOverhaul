using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompWeaponOil: ThingComp
    {
        public int oilCharges = 20; // 1 in-game day
        public int maxCharges = 20;
        public HediffDef hediffDef;
        public string oilType;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref oilCharges, "oilCharges", 0);
        }

        public void RefreshOil(int charges, string oilName, HediffDef oilHediff)
        {
            oilType = oilName;
            hediffDef = oilHediff;
            oilCharges = charges; // Reset duration
        }

        public void UseCharge()
        {
            oilCharges--;
        }

        public override string CompInspectStringExtra()
        {
            return "DankPyon_OilWeaponRemaining".Translate(oilCharges);
        }
    }

}
