using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class CompCreatesInfestations_Mine : ThingComp
    {
        private int lastCreatedInfestationTick = -999999;

        private const float MinRefireDays = 7f;

        private const float PreventInfestationsDist = 10f;

        public bool CanCreateInfestationNow
        {
            get
            {
                if (CantFireBecauseCreatedInfestationRecently)
                {
                    return false;
                }
                if (CantFireBecauseSomethingElseCreatedInfestationRecently)
                {
                    return false;
                }
                if (parent is Building_WorkTable workTable)
                {
                    foreach (var reservation in this.parent.Map.reservationManager.ReservationsReadOnly)
                    {
                        if (reservation.Target.Thing == this.parent)
                        {
                            var pawn = reservation.Claimant;
                            // Check if the pawn is not moving and is at the interaction cell
                            if (!pawn.pather.MovingNow && pawn.Position == workTable.InteractionCell)
                            {
                                return true; // Return true if any pawn meets the requirement
                            }
                        }
                    }
                }
                return false;
            }

        }
        public bool CantFireBecauseCreatedInfestationRecently => Find.TickManager.TicksGame <= lastCreatedInfestationTick + (60000 * MinRefireDays);

        public bool CantFireBecauseSomethingElseCreatedInfestationRecently
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                List<Thing> list = parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.CreatesInfestations);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != parent && list[i].Position.InHorDistOf(parent.Position, PreventInfestationsDist) && list[i].TryGetComp<CompCreatesInfestations_Mine>().CantFireBecauseCreatedInfestationRecently)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lastCreatedInfestationTick, "lastCreatedInfestationTick", -999999);
        }

        public void Notify_CreatedInfestation()
        {
            lastCreatedInfestationTick = Find.TickManager.TicksGame;
        }

    }
}
