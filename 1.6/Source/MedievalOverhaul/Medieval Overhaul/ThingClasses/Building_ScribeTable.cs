using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
    public class Building_ScribeTable : Building_CommsConsole
    {
        private CompAffectedByFacilities facilities;
        private RequireLinkables extension;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            facilities = this.TryGetComp<CompAffectedByFacilities>();
            extension = def.GetModExtension<RequireLinkables>();
            if (extension == null)
            {
                Log.Error(def + " " + "does not have the required Mod Extesion.");
            }
            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
            if (CanUseCommsNow)
            {
                LongEventHandler.ExecuteWhenFinished(new Action(AnnounceTradeShips));
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            FloatMenuOption failureReason = GetActualFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }
            foreach (ICommunicable communicable in GetCommTargets_Messenger(myPawn))
            {
                FloatMenuOption floatMenuOption = communicable.CommFloatMenuOption(this, myPawn);
                if (floatMenuOption != null)
                {
                    yield return floatMenuOption;
                }
            }
            yield break;
        }

        public IEnumerable<ICommunicable> GetCommTargets_Messenger(Pawn myPawn)
        {
            List<ICommunicable> targets = [];
            // Add valid factions
            if (Find.FactionManager != null)
            {
                // Iterate directly over IEnumerable instead of converting to List
                foreach (Faction f in Find.FactionManager.AllFactionsVisibleInViewOrder)
                {
                    if (!f.temporary && !f.IsPlayer)
                    {
                        targets.Add(f);
                    }
                }
            }

            return targets;
        }

        public FloatMenuOption GetActualFailureReason(Pawn myPawn)
        {
            if (facilities.LinkedFacilitiesListForReading.Count < extension.linkablesNeeded)
            {
                return new FloatMenuOption("Missing required building", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            for (int i = 0; i < facilities.LinkedFacilitiesListForReading.Count; i++)
            {
                if (facilities.LinkedFacilitiesListForReading[i] is Building building)
                {
                    var fuelComp = building.TryGetComp<CompRefuelableCustom>();
                    if (fuelComp != null && !fuelComp.HasFuel)
                    {
                        return new FloatMenuOption("Required building needs fuel", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    }
                }
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            bool hasCommTarget = false;
            foreach (ICommunicable target in GetCommTargets_Messenger(myPawn))
            {
                hasCommTarget = true;
                break; // Exit early if at least one target exists
            }

            if (!hasCommTarget)
            {
                return new FloatMenuOption("CannotUseReason".Translate("NoCommsTarget".Translate()), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }

            if (!CanActuallyUseCommsNow)
            {
                Log.Error(myPawn + " could not use comm console for unknown reason.");
                return new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            return null;
        }
        public bool CanActuallyUseCommsNow => Spawned && (powerComp == null || powerComp.PowerOn);

    }
}
