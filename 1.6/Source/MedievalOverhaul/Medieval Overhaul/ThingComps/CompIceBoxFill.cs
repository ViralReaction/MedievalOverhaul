using Verse;
using ProcessorFramework;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace MedievalOverhaul
{
    public class CompIceBoxFill : ThingComp
    {
        private Thing _parent;
        private Map _map;
        private CompProcessor _compProcessor;
        private float _rainAccumulated = 0f;
        private const float RAINTHRESHOLD = 5f;
        private const float SNOWMULTIPLIER = 1f;
        public Job activeJob = null;
        public Pawn jobPawn = null;
        public bool jobStarted = false; // Track if a job is currently active
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            _parent = this.parent;
            _map = _parent.Map;
            _compProcessor = _parent.TryGetComp<CompProcessor>();
            if (_compProcessor == null)
            {
                Log.Error("Medieval Overhaul: " + _parent.def + " " + "is missing CompProcessor and will be despawned.");
                _parent.Destroy();
            }
        }
        public override void CompTickRare()
        {
            if (_compProcessor.SpaceLeft == 0) return;
            if (_map.weatherManager.RainRate == 0 && _map.weatherManager.SnowRate == 0) return;
            if (!_parent.IsOutside() || _parent.Position.Roofed(_map)) return;


            float rainRate = _map.weatherManager.RainRate;
            float snowRate = _map.weatherManager.SnowRate;
            _rainAccumulated += rainRate + (snowRate * SNOWMULTIPLIER);
            if (_rainAccumulated < RAINTHRESHOLD) return;

            _rainAccumulated = 0f;

            var processList = _compProcessor.enabledProcesses;
            if (processList.Count == 0) return;
            foreach (var processDef in processList)
            {
                var process = processDef.Key;
                var ingredientFilter = processDef.Value;
                ThingDef ingredient = null;
                for (int i = 0; i < ingredientFilter.allowedIngredients.Count; i++)
                {
                    if (ingredientFilter.allowedIngredients[i] != null)
                    {
                        ingredient = ingredientFilter.allowedIngredients[i];
                        break;
                    }
                }
                if (ingredient == null) return;
                Thing thing = ThingMaker.MakeThing(ingredient);
                thing.stackCount = 1;
                _compProcessor.AddIngredient(thing, process);
                break;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = jobStarted ? "Cancel Retrieve Job" : "Retrieve Water",
                action = delegate ()
                {
                    if (jobStarted)
                    {

                        if (activeJob != null && jobPawn != null)
                        {
                            if (jobPawn.jobs.curJob == activeJob)
                            {
                                jobPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                            }
                        }
                        jobStarted = false;
                        activeJob = null;
                        jobPawn = null;
                        return;
                    }
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    Job haulJob = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_IceMold_Empty, this.parent);

                    Pawn hauler = Utility.FindBestHauler(this.parent);
                    if (hauler != null)
                    {
                        jobStarted = true;
                        activeJob = haulJob;
                        jobPawn = hauler;
                        hauler.jobs.StartJob(haulJob, JobCondition.InterruptForced);
                    }
                },
                icon = ContentFinder<Texture2D>.Get("UI/GatherWater")
            };
        }
    }
}
