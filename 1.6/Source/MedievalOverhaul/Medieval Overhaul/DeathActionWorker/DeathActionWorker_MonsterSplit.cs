using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MedievalOverhaul
{
	public class DeathActionWorker_MonsterSplit : DeathActionWorker
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
	        if (corpse.Map == null) return;
	        DeathSplitExtension extensionProps = corpse.InnerPawn.def.GetModExtension<DeathSplitExtension>();
	        if (extensionProps == null || !Rand.Chance(extensionProps.randomChance)) return;
	        int range = extensionProps.createAmount.RandomInRange;
	        List <Thing> spawnedThings = [];
	        for (int i = 0; i < range; i++)
	        {
		        PawnKindDef pawnKind = extensionProps.pawnKindDef ?? MedievalOverhaulDefOf.DankPyon_SchratDark_Sapling;
		        Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, null);
		        pawn.ageTracker.AgeBiologicalTicks = 60;
		        spawnedThings.Add(pawn);
	        }
	        if (extensionProps.explosion)
	        {
		        DamageDef damageDef = extensionProps.damageDef ?? MedievalOverhaulDefOf.DankPyon_SchratCollapse;
		        GenExplosion.DoExplosion(corpse.Position, corpse.Map, extensionProps.explosionRadius, damageDef, corpse.InnerPawn, extensionProps.explosionDamage, extensionProps.armorPen, null, null, null, null, null, 0f, 1, null, null, 255, false, null, 0f, 1, 0f, true, null, spawnedThings, null, true, 1f, 0f, true, null, 1f, null, null);
	        }
	        foreach (Thing thing in spawnedThings)
	        {
		        Pawn pawn = thing as Pawn;
		        IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(corpse.Position, corpse.Map, 3);
		        GenSpawn.Spawn(pawn, intVec3, corpse.Map, Rot4.Random, WipeMode.Vanish, false);
		        pawn?.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, false, null, false, false, false);
	        }

        }
	}
}
