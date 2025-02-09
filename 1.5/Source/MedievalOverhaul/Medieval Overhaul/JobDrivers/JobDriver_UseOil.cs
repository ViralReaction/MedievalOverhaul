using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace MedievalOverhaul
{
    public class JobDriver_UseOil : JobDriver
    {

        #region Fields
        private int useDuration = 600;

        #endregion Fields

        #region Properties

        private ThingWithComps OilSource =>  TargetThingB as ThingWithComps;
        private TargetIndex Pawn => TargetIndex.A;
        private TargetIndex TargetOil => TargetIndex.B;
        

        #endregion Properties

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            this.FailOn(() => !base.TargetThingB.TryGetComp<CompUsable>().CanBeUsedBy(this.pawn, false, false));
            Toil waitToil = new ()
            {
                actor = pawn
            };
            waitToil.initAction = () => waitToil.actor.pather.StopDead();
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
            waitToil.defaultDuration = 300;
            yield return waitToil.WithProgressBarToilDelay(Pawn);
            Toil addOil = new ();
            addOil.AddFinishAction(() => UseEffect());
            yield return addOil;
            yield break;
        }

        public void UseEffect()
        {

            CompUseEffect_AddOil compUsable = this.job.GetTarget(TargetOil).Thing.TryGetComp<CompUseEffect_AddOil>();
            OilSource.stackCount--;
            if (OilSource.stackCount == 0)
            {
                OilSource.Destroy();
            }
            compUsable.DoEffect(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

    }
}
