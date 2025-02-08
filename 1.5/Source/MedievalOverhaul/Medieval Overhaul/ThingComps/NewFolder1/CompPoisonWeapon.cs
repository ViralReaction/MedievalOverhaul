using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompPoisonWeapon : ThingComp
    {
        public int poisonCharges = 20; // 1 in-game day
        public int maxPoisonCharges = 20;
        public HediffDef hediffDef;
        public string oilType;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref poisonCharges, "poisonCharges", 0);
        }

        public void RefreshPoisonDuration()
        {
            poisonCharges = 20; // Reset duration
        }

        public void UseCharge()
        {
            poisonCharges--;
        }

        public override string CompInspectStringExtra()
        {
            return "DankPyon_OilWeaponRemaining".Translate(poisonCharges);
        }
    }

}
