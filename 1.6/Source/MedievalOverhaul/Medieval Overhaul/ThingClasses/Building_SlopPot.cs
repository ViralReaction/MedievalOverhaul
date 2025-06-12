using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MedievalOverhaul
{

	public class Building_SlopPot : Building_NutrientPasteDispenser
	{
		public CompRefuelable fuelComp;

		public int lastFueledTick = -999;

		public CompRefuelableStat nutritionComp;
		public CompSlop slopComp;
		public float nutritionAmount;
        private ThingDef _cachedDispensedDef;


        public Job activeJob = null;
		public Pawn jobPawn = null;
        public bool jobStarted = false; // Track if a job is currently active


        public bool CanActuallyDispenseNow
		{
			get
			{
				return this.HasEnoughFeedstockInHoppers();
			}
		}


        public override ThingDef DispensableDef
        {
            get
            {
                if (_cachedDispensedDef == null)
                {
                    slopComp ??= GetComp<CompSlop>(); // Ensure slopComp is assigned before using it
                    _cachedDispensedDef = slopComp?.mealdef ?? ThingDefOf.MealNutrientPaste;
                }
                return _cachedDispensedDef;
            }
        }




        public override bool HasEnoughFeedstockInHoppers()
		{
			return nutritionComp.Fuel >= def.building.nutritionCostPerDispense;
		}


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			fuelComp = GetComp<CompRefuelable>();
			nutritionComp = GetComp<CompRefuelableStat>();
			slopComp = GetComp<CompSlop>();
			if (slopComp == null) throw new Exception($"{def.defName} does not have CompProperties_Slop");
			if (nutritionComp == null)
				throw new Exception(
					$"{def.defName} does not have CompProperties_RefuelableStat with with at least one food category defined");
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
		}

		public override void Tick()
		{
			base.Tick();

			this.fuelComp.Notify_UsedThisTick();
			var ticks = Find.TickManager.TicksGame;
			if (ticks % slopComp.Props.fuelCheckTicks != 0) return;
			if (fuelComp.HasFuel) lastFueledTick = ticks;
			if (lastFueledTick < 0 || ticks - lastFueledTick <= slopComp.Props.unfueledTicksToRot) return;
			nutritionComp.ConsumeFuel(nutritionComp.Fuel);
			slopComp.ingredients.Clear();
			Messages.Message("DankPyon_MealSlop_Spoiled".TranslateSimple().CapitalizeFirst(),
				new TargetInfo(PositionHeld, MapHeld), MessageTypeDefOf.NegativeEvent);
		}

		public override Thing TryDispenseFood()
		{
			if (!CanActuallyDispenseNow)
				return null;
			nutritionComp.ConsumeFuel(def.building.nutritionCostPerDispense);
			Thing meal = ThingMaker.MakeThing(slopComp.Props.mealDef);
			CompIngredients compIngredients = meal.TryGetComp<CompIngredients>();
			for (int i = 0; i < this.slopComp.ingredients.Count; i++)
			{
				compIngredients.RegisterIngredient(this.slopComp.ingredients[i]);
			}
			if (this.nutritionComp.Fuel == 0)
			{
				this.slopComp.ingredients.Clear();
			}
			def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map));
			return meal;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new();
			stringBuilder.AppendLine(base.GetInspectString());
			stringBuilder.Append(
				"DankPyon_StewMealAmount".Translate() + ": " +
				(int)Math.Round(nutritionComp.Fuel / def.building.nutritionCostPerDispense) + "/" +
					MaxMeals());
			stringBuilder.AppendLine(" | " +
				"DankPyon_StewPot_NutritionStored".Translate() + ": " +
				(int)Math.Round(nutritionComp.Fuel));
			if (!this.fuelComp.HasFuel && this.nutritionComp.HasFuel)
			{
				stringBuilder.AppendLine("DankPyon_SlopRotting".Translate());
			}
			else if (this.slopComp.ingredients.Count > 0)
			{
				stringBuilder.AppendLine("Ingredients".Translate() + ": ");
				stringBuilder.Append(this.slopComp.GetIngredientsString(false, out var _));
			}
            return stringBuilder.ToString().Trim();
		}

		public virtual int MaxMeals()
		{
			return (int)Math.Round(nutritionComp.TargetFuelLevel / def.building.nutritionCostPerDispense);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastFueledTick, "lastFueledTick", -999);

		}

		public bool Accepts(Thing t)
		{
			return nutritionComp.Props.fuelFilter.Allows(t);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				if (!(gizmo is Designator_Build dbGizmo && dbGizmo.PlacingDef == ThingDefOf.Hopper))
					yield return gizmo;
			yield return new Command_RetrieveSlop(this, slopComp.Props.mealDef, 1)
			{
                defaultLabel = jobStarted ? "DankPyon_RetrieveSlopMeals_Cancel".Translate() : "DankPyon_RetrieveSlopMeals".Translate(1),
            };
            yield return new Command_RetrieveSlop(this, slopComp.Props.mealDef, 5)
            {
                defaultLabel = jobStarted ? "DankPyon_RetrieveSlopMeals_Cancel".Translate() : "DankPyon_RetrieveSlopMeals".Translate(5),
            };
            yield return new Command_RetrieveSlop(this, slopComp.Props.mealDef, 10)
            {
                defaultLabel = jobStarted ? "DankPyon_RetrieveSlopMeals_Cancel".Translate() : "DankPyon_RetrieveSlopMeals".Translate(10),
            };
            yield return new Command_Action
            {
				defaultLabel = jobStarted ? "DankPyon_EmptyPot_Cancel".Translate() : "DankPyon_EmptyPot".Translate(),
                action = delegate ()
				{
                    if (jobStarted)
                    {
                        if (activeJob != null && jobPawn != null && jobPawn.jobs.curJob == activeJob)
                        {
                            jobPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        }
                        jobStarted = false;
                        activeJob = null;
                        jobPawn = null;
                        return;
                    }
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    Job haulJob = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_EmptyPot, this);
                    haulJob.count = 1;

                    Pawn hauler = Utility.FindBestHauler(this);
                    if (hauler != null)
                    {
                        jobStarted = true;
                        activeJob = haulJob;
                        jobPawn = hauler;
                        hauler.jobs.StartJob(haulJob, JobCondition.InterruptForced);
                    }

                },
				icon = ContentFinder<Texture2D>.Get("UI/EmptyPot_Icon")
			};
		}

        private static List<Pawn> canReach = [];
        private static List<Pawn> canUse = [];
        private static List<Pawn> allowedPawn = [];

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (this.comps != null && this.comps.Count > 0)
            {
                for (int i = 0; i < this.comps.Count; i++)
                {
                    foreach (FloatMenuOption floatMenuOption2 in this.comps[i].CompFloatMenuOptions(selPawn))
                    {
                        yield return floatMenuOption2;
                    }
                }
            }
            if (!CanActuallyDispenseNow)
            {
                yield return new FloatMenuOption("DankPyon_NoFood".Translate(this), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("DankPyon_CannotUse".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            if (!this.IsCapableOfUsing(selPawn))
            {
                yield return new FloatMenuOption("DankPyon_CannotUse".Translate(this.Label) + ": " + "Incapable".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            yield return new FloatMenuOption("DankPyon_GetSlop".Translate(slopComp.mealdef), delegate ()
            {
                // Create a job for the first pawn to get food from THIS dispenser
                Job job = JobMaker.MakeJob(JobDefOf.Ingest, this); // 'this' refers to the dispenser
                job.count = 1; // Ensure only one meal is taken
                selPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.SatisfyingNeeds), false);
            },
                MenuOptionPriority.Default,
                null, null, 0f, null, null, true, 0);
            yield break;
        }


        // public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        // {
        //     foreach (FloatMenuOption floatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
        //     {
        //         yield return floatMenuOption;
        //     }
        //
        //     if (!CanActuallyDispenseNow)
        //     {
        //         yield return new FloatMenuOption("DankPyon_NoFood".Translate(this), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
        //         yield break;
        //     }
        //     canReach.Clear();
        //     canUse.Clear();
        //     for (int i = 0; i < selPawns.Count; i++)
        //     {
        //         if (selPawns[i].RaceProps.Humanlike)
        //         {
        //             if (selPawns[i].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
        //             {
        //                 canReach.Add(selPawns[i]);
        //             }
        //             if (this.IsCapableOfUsing(selPawns[i]))
        //             {
        //                 canUse.Add(selPawns[i]);
        //             }
        //         }
        //     }
        //     if (canReach.Count == 0)
        //     {
        //         yield return new FloatMenuOption("DankPyon_CannotUse".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
        //         yield break;
        //     }
        //     if (canUse.Count == 0)
        //     {
        //         yield return new FloatMenuOption("DankPyon_CannotUse".Translate(this.Label) + ": " + "Incapable".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
        //         yield break;
        //     }
        //     allowedPawn.Clear();
        //     for (int i = 0; i < canReach.Count; i++)
        //     {
        //         if (canUse.Contains(canReach[i]))
        //         {
        //             allowedPawn.Add(canUse[i]);
        //         }
        //     }
        //     if (allowedPawn.Count > 0)
        //     {
        //         yield return new FloatMenuOption("DankPyon_GetSlop".Translate(slopComp.mealdef), delegate ()
        //         {
        //             for (int i = 1; i < allowedPawn.Count; i++)
        //             {
        //                 Job job = JobMaker.MakeJob(JobDefOf.Ingest, this); // 'this' refers to the dispenser
        //                 job.count = 1; // Ensure only one meal is taken
        //                 allowedPawn[i].jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.SatisfyingNeeds), false);
        //             }
        //         },
        //         MenuOptionPriority.Default,
        //         null, null, 0f, null, null, true, 0);
        //     }
        //     yield break;
        //
        // }

        private bool IsCapableOfUsing(Pawn pawn)
        {
            return !pawn.IsMutant && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        }

        private Texture2D GetMealTexture(ThingDef mealDef, int stackSize = 1)
        {
            if (mealDef == null || mealDef.graphic == null)
                return null;

            // If the item has stack-based graphics, get the correct texture
            if (mealDef.graphic is Graphic_StackCount stackGraphic)
            {
                Graphic subGraphic = stackGraphic.SubGraphicForStackCount(stackSize, mealDef);
                if (subGraphic != null)
                {
                    return (Texture2D)subGraphic.MatSingle.mainTexture;
                }
            }
            else // If not stack-based, just return the main texture
            {
                return (Texture2D)mealDef.graphic.MatSingle.mainTexture;
            }

            return null; // Return null if no valid texture found
        }
    }
}