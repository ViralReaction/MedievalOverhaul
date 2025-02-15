using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;

namespace MedievalOverhaul
{
    public class JobDriver_DispenseSlop : JobDriver
    {
        private const TargetIndex SlopTarget = TargetIndex.A;
        private const TargetIndex DispensedTarget = TargetIndex.B;
        private const TargetIndex StorageTarget = TargetIndex.C;
        private int Amount => job.count;
        private const int Duration = 120;
        protected Thing Dispenser => job.GetTarget(TargetIndex.A).Thing;

        public override void SetInitialPosture()
        {
            base.SetInitialPosture();
            this.AddFinishAction((JobCondition jobCondition) =>
            {
                if (TargetA.Thing is Building_SlopPot building)
                {
                    ((Building_SlopPot)building).jobStarted = false; // Reset job state
                    ((Building_SlopPot)building).activeJob = null;
                    ((Building_SlopPot)building).jobPawn = null;
                }
            });
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            var things = Dispenser as Building_SlopPot;
            this.FailOn(() => (Dispenser is not Building_SlopPot thing) || (!thing.CanDispenseNow));
            this.FailOnDestroyedNullOrForbidden(SlopTarget);
            yield return Toils_Goto.GotoThing(SlopTarget, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(SlopTarget).WithProgressBarToilDelay(SlopTarget);

            Toil collect = new Toil
            {
                initAction = () =>
                {
                    Thing dispensedFood = null;
                    for (int i = 0; i < Amount; i++)
                    {
                        dispensedFood = things.TryDispenseFood();
                        if (dispensedFood == null || dispensedFood.stackCount == 0)
                        {
                            EndJobWith(JobCondition.Succeeded);
                            return;
                        }
                        GenPlace.TryPlaceThing(dispensedFood, things.InteractionCell, this.Map, ThingPlaceMode.Near);
                    }                  
                    StoragePriority storagePriority = StoreUtility.CurrentStoragePriorityOf(dispensedFood);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(dispensedFood, pawn, Map, storagePriority, pawn.Faction, out IntVec3 c))
                    {
                        job.SetTarget(DispensedTarget, dispensedFood);
                        job.count = dispensedFood.stackCount;
                        job.SetTarget(StorageTarget, c);
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }

                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return collect;
            yield return Toils_Reserve.Reserve(DispensedTarget);
            yield return Toils_Reserve.Reserve(StorageTarget);
            yield return Toils_Goto.GotoThing(DispensedTarget, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(DispensedTarget);
            Toil carry = Toils_Haul.CarryHauledThingToCell(StorageTarget);
            yield return carry;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageTarget, carry, true);
        }
    }
}
