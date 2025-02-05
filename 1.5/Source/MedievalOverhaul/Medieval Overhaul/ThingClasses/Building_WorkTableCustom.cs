using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Building_WorkTableCustom : Building, IBillGiver, IBillGiverWithTickAction
    {
        public BillStack billStack;

        private CompPowerTrader powerComp;

        private CompRefuelable refuelableComp;

        private CompBreakdownable breakdownableComp;

        private CompMoteEmitter moteEmitterComp;

        public bool CanWorkWithoutFuel
        {
            get
            {
                return this.fuelCompCustom == null;
            }
        }
        public bool CanWorkWithoutPower
        {
            get
            {
                if (powerComp == null)
                {
                    return true;
                }
                if (def.building.unpoweredWorkTableWorkSpeedFactor > 0f)
                {
                    return true;
                }
                return false;
            }
        }


        public BillStack BillStack => billStack;

        public IntVec3 BillInteractionCell => InteractionCell;

        public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);
        public Building_WorkTableCustom()
        {
            billStack = new BillStack(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref billStack, "billStack", this);
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            fuelCompCustom = GetComp<CompRefuelableCustom>();
            breakdownableComp = GetComp<CompBreakdownable>();
            moteEmitterComp = GetComp<CompMoteEmitter>();
            foreach (Bill item in billStack)
            {
                item.ValidateSettings();
            }
            
        }

        public virtual void UsedThisTick()
        {
            fuelCompCustom?.Notify_UsedThisTick();
            if (moteEmitterComp != null)
            {
                if (!moteEmitterComp.MoteLive)
                {
                    this.moteEmitterComp.Emit();
                }
                moteEmitterComp.Maintain();
            }
        }

        public bool CurrentlyUsableForBills()
        {
            if (!UsableForBillsAfterFueling())
            {
                return false;
            }
            if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
            {
                return false;
            }
            if (!CanWorkWithoutFuel && (fuelCompCustom == null || !fuelCompCustom.HasFuel))
            {
                return false;
            }
            return true;
        }
        public bool UsableForBillsAfterFueling()
        {
            if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
            {
                return false;
            }
            if (breakdownableComp != null && breakdownableComp.BrokenDown)
            {
                return false;
            }
            return true;
        }

        private CompRefuelableCustom fuelCompCustom;
        public virtual void Notify_BillDeleted(Bill bill)
        {
        }

    }
}
