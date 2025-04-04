using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
	public class JobDriver_RefuelCustom : JobDriver
	{
		private const TargetIndex RefuelableInd = TargetIndex.A;

		private const TargetIndex FuelInd = TargetIndex.B;

		private const int RefuelingDuration = 240;

		protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

		protected CompRefuelableCustom RefuelableComp => Refuelable.TryGetComp<CompRefuelableCustom>();

		protected Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed)
				   && pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
		}

        public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			AddEndCondition(() => !RefuelableComp.IsFull ? JobCondition.Ongoing : JobCondition.Succeeded);
			AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
			AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);
			yield return Toils_General.DoAtomic(delegate { job.count = RefuelableComp.GetFuelCountToFullyRefuel(Fuel); });
			Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
			yield return reserveFuel;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
				.FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul
				.StartCarryThing(TargetIndex.B, false, true)
				.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None,
				true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B)
				.FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return FinalizeRefueling(TargetIndex.A, TargetIndex.B);
		}

		public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
		{
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = () =>
            {
                Job curJob = toil.actor.CurJob;
                Thing thing = curJob.GetTarget(refuelableInd).Thing;
                CompRefuelableCustom refuelableComp = thing.TryGetComp<CompRefuelableCustom>();

                if (refuelableComp == null)
                    return;

                if (toil.actor.CurJob.placedThings.NullOrEmpty())
                {
                    refuelableComp.Refuel(new List<Thing> { curJob.GetTarget(fuelInd).Thing });
                }
                else
                {
                    List<Thing> refuelThings = new List<Thing>();
                    foreach (var placedThing in toil.actor.CurJob.placedThings)
                    {
                        refuelThings.Add(placedThing.thing);
                    }
                    refuelableComp.Refuel(refuelThings);
                }
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
	}
}