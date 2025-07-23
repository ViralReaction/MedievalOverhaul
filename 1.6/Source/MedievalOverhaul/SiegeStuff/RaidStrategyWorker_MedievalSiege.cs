using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace MedievalOverhaul;

public class RaidStrategyWorker_MedievalSiege : RaidStrategyWorker_Siege
{
    public override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        IntVec3 siegeSpot = RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld, map);
        float num = parms.points * Rand.Range(0.2f, 0.3f);
        if (num < SiegeUtility.ArtyCost)
        {
            num = SiegeUtility.ArtyCost;
        }
        return new LordJob_MedievalSiege(parms.faction, siegeSpot, num);
    }

    public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        if (!base.CanUseWith(parms, groupKind))
        {
            return false;
        }
        FactionSiegeExtension extension = parms.faction.def.GetModExtension<FactionSiegeExtension>();
        if (extension is not { medievalSiege: true })
        {
            return false;
        }
        return parms.faction.def.techLevel == TechLevel.Medieval;
    }

    public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
    {
        if (g.kind.RaceProps.Animal)
        {
            return false;
        }
        return base.CanUsePawnGenOption(pointsTotal, g, chosenGroups, faction);
    }
}
