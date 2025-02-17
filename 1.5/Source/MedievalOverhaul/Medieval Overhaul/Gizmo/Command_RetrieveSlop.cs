using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse;
using Verse.Sound;
using System.Collections.Generic;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Command_RetrieveSlop : Command_Action
    {
        private bool jobStarted;
        private Job activeJob;
        private Pawn jobPawn;
        private Building_SlopPot parentBuilding;
        private ThingDef mealDef;
        private int dispenseSize;

        public Command_RetrieveSlop(Building_SlopPot parent, ThingDef meal, int size)
        {
            parentBuilding = parent;
            mealDef = meal;
            dispenseSize = size;
            defaultLabel = jobStarted ? "Retrieving Food..." : "Retrieve Food";
            icon = GetMealTexture(mealDef, size);
            action = ToggleJob;
        }

        private void ToggleJob()
        {
            if (jobStarted)
            {
                if (activeJob != null && jobPawn != null && jobPawn.jobs.curJob == activeJob)
                {
                    jobPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                parentBuilding.jobStarted = false;
                parentBuilding.activeJob = null;
                parentBuilding.jobPawn = null;
                return;
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            Job haulJob = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_DispenseSlop, parentBuilding);
            haulJob.count = dispenseSize;

            Pawn hauler = Utility.FindBestHauler(parentBuilding);
            if (hauler != null)
            {
                parentBuilding.jobStarted = true;
                parentBuilding.activeJob = haulJob;
                parentBuilding.jobPawn = hauler;
                hauler.jobs.StartJob(haulJob, JobCondition.InterruptForced);
            }
        }
        private Texture2D GetMealTexture(ThingDef mealDef, int stackSize)
        {
            if (mealDef == null || mealDef.graphic == null) return null;

            if (mealDef.graphic is Graphic_StackCount stackGraphic)
            {
                Graphic subGraphic = stackGraphic.SubGraphicForStackCount(stackSize, mealDef);
                return subGraphic != null ? (Texture2D)subGraphic.MatSingle.mainTexture : null;
            }
            else
            {
                return (Texture2D)mealDef.graphic.MatSingle.mainTexture;
            }
        }
       
        public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
        private static readonly Texture2D BGTexActive = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
    }

}