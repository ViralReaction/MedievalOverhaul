using System.Collections.Generic;
using Verse.AI;
using Verse;
using ProcessorFramework;
using RimWorld;

namespace MedievalOverhaul
{
    public class JobDriver_EmptyIceMold : JobDriver
    {
        private const TargetIndex IceMoldBox = TargetIndex.A;
        private const TargetIndex DispensedTarget = TargetIndex.B;
        private const TargetIndex StorageTarget = TargetIndex.C;

        private int Amount => job.count;
        private const int Duration = 180;
        protected Thing Dispenser => job.GetTarget(TargetIndex.A).Thing;

        public override void SetInitialPosture()
        {
            base.SetInitialPosture();
            this.AddFinishAction((JobCondition jobCondition) =>
            {
                var comp = TargetA.Thing.TryGetComp<CompIceBoxFill>();
                if (comp != null)
                {
                    comp.jobStarted = false; // Reset job state
                    comp.activeJob = null;
                    comp.jobPawn = null;
                }
            });
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            CompProcessor comp = Dispenser.TryGetComp<CompProcessor>();
            if (comp == null)
            {
                Log.Error("[DEBUG] CompProcessor is NULL!");
                yield break;
            }


            this.FailOnDestroyedNullOrForbidden(IceMoldBox);

            yield return Toils_Goto.GotoThing(IceMoldBox, PathEndMode.ClosestTouch);

            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(IceMoldBox).WithProgressBarToilDelay(IceMoldBox);

            Toil collect = new Toil
            {
                initAction = () =>
                {
                    Thing thing = null;
                    ActiveProcess activeProcess = comp.activeProcesses[0];
                    if (activeProcess == null)
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    for (int i = 0; i < comp.innerContainer.Count; i++)
                    {
                        Thing innerThing = ThingMaker.MakeThing(comp.innerContainer[i].def, null);
                        innerThing.stackCount = comp.innerContainer[i].stackCount;
                        thing = innerThing;
                        GenPlace.TryPlaceThing(thing, pawn.Position, this.Map, ThingPlaceMode.Near);
                    }
                    comp.activeProcesses.Remove(activeProcess);
                    if (comp.activeProcesses.Count == 0)
                    {
                        comp.innerContainer.Clear();
                    }
                    if (thing == null || thing.stackCount == 0)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                    StoragePriority storagePriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, storagePriority, pawn.Faction, out IntVec3 c))
                    {
                        job.SetTarget(DispensedTarget, thing);
                        job.count = thing.stackCount;
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
