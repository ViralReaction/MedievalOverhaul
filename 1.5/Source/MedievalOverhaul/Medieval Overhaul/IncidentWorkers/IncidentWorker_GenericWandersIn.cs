using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    internal class IncidentWorker_GenericWandersIn : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            IncidentProperties incidentProperties = IncidentProperties.Get((Def)this.def);
            if (!base.CanFireNowSub(parms) && incidentProperties != null && incidentProperties.kindDef != null)
                return false;
            Map target = (Map)parms.target;
            return !target.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(incidentProperties.kindDef.race) && this.TryFindEntryCell(target, out IntVec3 _);
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            IncidentProperties incidentProperties = IncidentProperties.Get((Def)this.def);
            Map target = (Map)parms.target;
            if (!this.TryFindEntryCell(target, out IntVec3 cell))
                return false;
            int numPawns = Mathf.Clamp(GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget)target) / incidentProperties.kindDef.combatPower), 1, incidentProperties.max.RandomInRange);
            int exitTickOffset = Rand.RangeInclusive(90000, 150000);
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, target, 10f, out IntVec3 forcedGotoCell))
                forcedGotoCell = IntVec3.Invalid;
            Pawn pawn = (Pawn)null;
            for (int index = 0; index < numPawns; ++index)
            {
                IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(cell, target, 10);
                pawn = PawnGenerator.GeneratePawn(incidentProperties.kindDef);
                GenSpawn.Spawn((Thing)pawn, intVec3, target, Rot4.Random, WipeMode.Vanish, false);
                if (incidentProperties.leaveMapAfterTime)
                    pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + exitTickOffset;
                if (forcedGotoCell.IsValid)
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(forcedGotoCell, target, 10);
            }
            Find.LetterStack.ReceiveLetter(this.def.letterLabel.Formatted((NamedArgument)incidentProperties.kindDef.label).CapitalizeFirst(), this.def.letterText.Formatted((NamedArgument)numPawns, (NamedArgument)incidentProperties.kindDef.label), this.def.letterDef, (LookTargets)(Thing)pawn, (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell) => RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
    }
}
