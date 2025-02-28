using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;

namespace MedievalOverhaul
{
    public class JobDriver_EmptySlopPot : JobDriver
    {
        private const TargetIndex SlopTarget = TargetIndex.A;
        private int Amount => job.count;
        private const int Duration = 240;
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
            this.FailOn(() => (Dispenser is not Building_SlopPot thing) || (!thing.CanActuallyDispenseNow));
            this.FailOnDestroyedNullOrForbidden(SlopTarget);
            yield return Toils_Reserve.Reserve(SlopTarget);
            yield return Toils_Goto.GotoThing(SlopTarget, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(SlopTarget).WithProgressBarToilDelay(SlopTarget);

            Toil collect = new Toil
            {
                initAction = () =>
                {

                    things.nutritionComp.ConsumeFuel(things.nutritionComp.Fuel);
                    things.slopComp.ingredients.Clear();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return collect;
        }
    }
}
