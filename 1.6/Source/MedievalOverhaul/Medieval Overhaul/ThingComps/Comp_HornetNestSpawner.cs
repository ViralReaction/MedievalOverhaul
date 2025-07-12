using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using Verse.AI.Group;
using Verse.Sound;
using Verse;

namespace MedievalOverhaul
{
    public class Comp_HornetNestSpawner : ThingComp
    {
        public int nextPawnSpawnTick = -1;
        public bool aggressive = true;
        public bool canSpawnPawns = true;

        private CompProperties_HornetNestSpawner Props => (CompProperties_HornetNestSpawner)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad || this.nextPawnSpawnTick != -1)
                return;
            this.SpawnInitialPawns();
        }

        private void SpawnInitialPawns()
        {
            int num = 0;
            while (num < this.Props.initialPawnCount && this.TrySpawnPawn(out Pawn _))
                ++num;
            this.CalculateNextPawnSpawnTick();
        }

        private void CalculateNextPawnSpawnTick() => this.CalculateNextPawnSpawnTick(this.Props.pawnSpawnIntervalDays.RandomInRange * 60000f);

        public void CalculateNextPawnSpawnTick(float delayTicks) => this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)((double)delayTicks / (1.0 * (double)Find.Storyteller.difficulty.enemyReproductionRateFactor));

        private bool TrySpawnPawn(out Pawn pawn)
        {
            pawn = null;

            if (this.parent == null)
            {
                return false;
            }

            if (this.parent.Map == null)
            {
                return false;
            }

            if (this.Props == null)
            {
                return false;
            }

            if (this.Props.spawnablePawnKinds == null)
            {
                return false;
            }

            int num = 0;
            HashSet<string> uniqueDefNames = new HashSet<string>();

            foreach (string defName in this.Props.spawnablePawnKinds)
            {
                if (uniqueDefNames.Add(defName))
                {
                    ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                    if (thingDef == null)
                    {
                        continue;
                    }

                    var things = this.parent.Map.listerThings.ThingsOfDef(thingDef);
                    num += things.Count;
                }
            }

            if (num < Props.maxPawnCount)
            {
                string selectedDefName = this.Props.spawnablePawnKinds.RandomElement();

                PawnKindDef named = DefDatabase<PawnKindDef>.GetNamed(selectedDefName, false);
                if (named != null)
                {
                    Faction faction = Props.faction != null ? FactionUtility.DefaultFactionFrom(Props.faction) : null;

                    PawnGenerationRequest pawnRequest = new PawnGenerationRequest(named, faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, false, false);
                    Pawn pawnToCreate = PawnGenerator.GeneratePawn(pawnRequest);

                    IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(this.parent.Position, this.parent.Map, this.Props.pawnSpawnRadius);
                    GenSpawn.Spawn(pawnToCreate, spawnCell, this.parent.Map);

                    if (this.parent.Map != null)
                    {
                        Lord lord = null;

                        var factionPawns = faction != null ? this.parent.Map.mapPawns.SpawnedPawnsInFaction(faction) : null;

                        if (factionPawns != null && factionPawns.Any(p => p != pawnToCreate))
                        {
                            Thing closest = GenClosest.ClosestThing_Global(this.parent.Position, factionPawns, 30f, p => p != pawnToCreate && ((Pawn)p).GetLord() != null);

                            if (closest != null)
                                lord = ((Pawn)closest).GetLord();
                        }

                        if (lord == null && faction != null)
                        {
                            lord = LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(this.parent.Position, 10f), this.parent.Map);
                        }

                        if (lord != null)
                        {
                            lord.AddPawn(pawnToCreate);
                        }
                    }

                    if (this.Props.spawnSound != null)
                    {
                        this.Props.spawnSound.PlayOneShot(SoundInfo.InMap(this.parent));
                    }

                    pawn = pawnToCreate;
                    return true;
                }

                return false;
            }
            canSpawnPawns = false;
            return false;
        }

        public override void CompTick()
        {
            if (this.parent.Spawned && this.nextPawnSpawnTick == -1)
                this.SpawnInitialPawns();
            if (!this.parent.Spawned || Find.TickManager.TicksGame < this.nextPawnSpawnTick)
                return;
            Pawn pawn;
            if (this.TrySpawnPawn(out pawn) && pawn.caller != null)
                pawn.caller.DoCall();
            this.CalculateNextPawnSpawnTick();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate ()
                    {
                        Pawn pawn;
                        this.TrySpawnPawn(out pawn);
                    }
                };
            }
            yield break;
        }
        public override string CompInspectStringExtra()
        {
            if (!canSpawnPawns)
                return (string)"DormantHiveNotReproducing".Translate();
            return this.canSpawnPawns ? (string)("HiveReproducesIn".Translate() + ": " + (this.nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()) : (string)null;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick");
            Scribe_Values.Look<bool>(ref this.aggressive, "aggressive");
            Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns");
        }

    }
}