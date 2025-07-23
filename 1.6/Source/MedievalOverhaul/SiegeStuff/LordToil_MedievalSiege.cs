using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace MedievalOverhaul;

public class LordToil_MedievalSiege : LordToil
{
	public Dictionary<Pawn, DutyDef> rememberedDuties = new Dictionary<Pawn, DutyDef>();

	private const float BaseRadiusMin = 20f;

	private const float BaseRadiusMax = 35f;

	private static readonly FloatRange MealCountRangePerRaider = new FloatRange(1f, 3f);

	private const int StartBuildingDelay = 360;

	private static readonly FloatRange BuilderCountFraction = new FloatRange(0.40f, FractionLossesToAssault);

	private const float FractionLossesToAssault = 0.8f;

	private const int InitialShellsPerCannon = 12;

	private const int ReplenishAtShells = 6;

	private const int ShellReplenishCount = 12;

	private const int ReplenishAtMeals = 6;

	private const int MealReplenishCount = 12;

	public override IntVec3 FlagLoc => Data.siegeCenter;

	private LordToilData_Siege Data => (LordToilData_Siege)data;

	private IEnumerable<Frame> Frames
	{
		get
		{
			LordToilData_Siege data = Data;
			float radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
			List<Thing> framesList = Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
			if (framesList.Count == 0)
			{
				yield break;
			}
			for (int i = 0; i < framesList.Count; i++)
			{
				Frame frame = (Frame)framesList[i];
				if (frame.Faction == lord.faction && (frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
				{
					yield return frame;
				}
			}
		}
	}

	public override bool ForceHighStoryDanger => true;

	public LordToil_MedievalSiege(IntVec3 siegeCenter, float blueprintPoints)
	{
		data = new LordToilData_Siege();
		Data.siegeCenter = siegeCenter;
		Data.blueprintPoints = blueprintPoints;
	}

	public override void Init()
	{
		base.Init();
		LordToilData_Siege lordToilData_Siege = Data;
		lordToilData_Siege.baseRadius = Mathf.InverseLerp(BaseRadiusMin, BaseRadiusMax, lord.ownedPawns.Count / 50f);
		lordToilData_Siege.baseRadius = Mathf.Clamp(lordToilData_Siege.baseRadius, BaseRadiusMin, BaseRadiusMax);
		List<Thing> list = [];
		foreach (Blueprint_Build item2 in SiegeUtility.PlaceBlueprints(lordToilData_Siege.siegeCenter, Map, lord.faction, lordToilData_Siege.blueprintPoints))
		{
			lordToilData_Siege.blueprints.Add(item2);
			foreach (ThingDefCountClass cost in item2.TotalMaterialCost())
			{
				Thing thing = list.FirstOrDefault( t => t.def == cost.thingDef);
				if (thing != null)
				{
					thing.stackCount += cost.count;
					continue;
				}
				Thing thing2 = ThingMaker.MakeThing(cost.thingDef);
				thing2.stackCount = cost.count;
				list.Add(thing2);
			}
			if (item2.def.entityDefToBuild is ThingDef turret)
			{
				ThingDef thingDef = SiegeUtility.TryFindRandomShellDef(turret);
				if (thingDef != null)
				{
					Thing thing3 = ThingMaker.MakeThing(thingDef);
					thing3.stackCount = InitialShellsPerCannon;
					list.Add(thing3);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].stackCount = Mathf.CeilToInt(list[i].stackCount * Rand.Range(1f, 1.2f));
		}
		List<List<Thing>> list2 = [];
		for (int j = 0; j < list.Count; j++)
		{
			while (list[j].stackCount > list[j].def.stackLimit)
			{
				int num = Mathf.CeilToInt(list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
				Thing thing4 = ThingMaker.MakeThing(list[j].def);
				thing4.stackCount = num;
				list[j].stackCount -= num;
				list.Add(thing4);
			}
		}
		List<Thing> list3 = [];
		for (int k = 0; k < list.Count; k++)
		{
			list3.Add(list[k]);
			if (k % 2 == 1 || k == list.Count - 1)
			{
				list2.Add(list3);
				list3 = [];
			}
		}
		List<Thing> list4 = [];
		int num2 = Mathf.RoundToInt(MealCountRangePerRaider.RandomInRange * (float)lord.ownedPawns.Count);
		for (int l = 0; l < num2; l++)
		{
			Thing item = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_MealBread);
			list4.Add(item);
		}
		list2.Add(list4);
		DropPodUtility.DropThingGroupsNear(lordToilData_Siege.siegeCenter, base.Map, list2);
		lordToilData_Siege.desiredBuilderFraction = BuilderCountFraction.RandomInRange;
	}

	public override void UpdateAllDuties()
	{
		LordToilData_Siege data = Data;
		if (lord.ticksInToil < StartBuildingDelay)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				SetAsDefender(lord.ownedPawns[i]);
			}
			return;
		}
		rememberedDuties.Clear();
		int num = Mathf.RoundToInt(lord.ownedPawns.Count * data.desiredBuilderFraction);
		if (num <= 0)
		{
			num = 1;
		}
		int num2 = (from b in Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
			where b.def.hasInteractionCell && b.Faction == lord.faction && b.Position.InHorDistOf(FlagLoc, data.baseRadius)
			select b).Count();
		if (num < num2)
		{
			num = num2;
		}
		int num3 = 0;
		for (int j = 0; j < lord.ownedPawns.Count; j++)
		{
			Pawn pawn = lord.ownedPawns[j];
			if (pawn.mindState.duty.def == DutyDefOf.Build)
			{
				rememberedDuties.Add(pawn, DutyDefOf.Build);
				SetAsBuilder(pawn);
				num3++;
			}
		}
		int num4 = num - num3;
		for (int k = 0; k < num4; k++)
		{
			if (lord.ownedPawns.Where( pa => !rememberedDuties.ContainsKey(pa) && CanBeBuilder(pa)).TryRandomElement(out Pawn result))
			{
				rememberedDuties.Add(result, DutyDefOf.Build);
				SetAsBuilder(result);
				num3++;
			}
		}
		for (int l = 0; l < lord.ownedPawns.Count; l++)
		{
			Pawn pawn2 = lord.ownedPawns[l];
			if (!rememberedDuties.ContainsKey(pawn2))
			{
				SetAsDefender(pawn2);
				rememberedDuties.Add(pawn2, DutyDefOf.Defend);
			}
		}
		if (num3 == 0)
		{
			lord.ReceiveMemo("NoBuilders");
		}
	}

	public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
	{
		UpdateAllDuties();
		base.Notify_PawnLost(victim, cond);
	}

	public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		base.Notify_ConstructionFailed(pawn, frame, newBlueprint);
		if (frame.Faction == lord.faction && newBlueprint != null)
		{
			Data.blueprints.Add(newBlueprint);
		}
	}

	private static bool CanBeBuilder(Pawn p)
	{
		if (p.WorkTypeIsDisabled(WorkTypeDefOf.Construction) || p.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter))
		{
			return false;
		}
		return true;
	}

	private void SetAsBuilder(Pawn p)
	{
		LordToilData_Siege lordToilData_Siege = Data;
		p.mindState.duty = new PawnDuty(DutyDefOf.Build, lordToilData_Siege.siegeCenter);
		p.mindState.duty.radius = lordToilData_Siege.baseRadius;
		int minLevel = Mathf.Max(ThingDefOf.Sandbags.constructionSkillPrerequisite, MedievalOverhaulDefOf.DankPyon_Artillery_Trebuchet.constructionSkillPrerequisite);
		p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(minLevel);
		p.workSettings.EnableAndInitialize();
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef workTypeDef = allDefsListForReading[i];
			if (workTypeDef == WorkTypeDefOf.Construction)
			{
				p.workSettings.SetPriority(workTypeDef, 1);
			}
			else
			{
				p.workSettings.Disable(workTypeDef);
			}
		}
	}

	private void SetAsDefender(Pawn p)
	{
		LordToilData_Siege lordToilData_Siege = Data;
		p.mindState.duty = new PawnDuty(DutyDefOf.Defend, lordToilData_Siege.siegeCenter);
		p.mindState.duty.radius = lordToilData_Siege.baseRadius;
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		LordToilData_Siege lordToilData_Siege = Data;
		if (lord.ticksInToil == StartBuildingDelay)
		{
			lord.CurLordToil.UpdateAllDuties();
		}
		if (lord.ticksInToil > StartBuildingDelay && lord.ticksInToil % 500 == 0)
		{
			UpdateAllDuties();
		}
		if (Find.TickManager.TicksGame % 500 != 0)
		{
			return;
		}
		if (!Frames.Where(frame => !frame.Destroyed).Any() && !lordToilData_Siege.blueprints.Where((Blueprint blue) => !blue.Destroyed).Any() && !base.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any((Thing b) => b.Faction == lord.faction && b.def.building.buildingTags.Contains("Artillery")))
		{
			lord.ReceiveMemo("NoArtillery");
			return;
		}
		int num = GenRadial.NumCellsInRadius(20f);
		int AmmoAmount = 0;
		int MealAmount = 0;
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = lordToilData_Siege.siegeCenter + GenRadial.RadialPattern[i];
			if (!c.InBounds(Map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing = thingList[j];
				if (thing.def.IsShell && thing.def.projectileWhenLoaded?.projectile != null && thing.def.projectileWhenLoaded.projectile.damageDef.harmsHealth)
				{
					AmmoAmount += thing.stackCount;
				}
				if (thing.def == ThingDefOf.MealSurvivalPack)
				{
					MealAmount += thing.stackCount;
				}
			}
		}
		if (AmmoAmount < ReplenishAtShells)
		{
			ThingDef thingDef = SiegeUtility.TryFindRandomShellDef(MedievalOverhaulDefOf.DankPyon_Turret_Trebuchet);
			if (thingDef != null)
			{
				DropSupplies(thingDef, ShellReplenishCount);
			}
		}
		if (MealAmount < ReplenishAtMeals)
		{
			DropSupplies(ThingDefOf.MealSurvivalPack, MealReplenishCount);
		}
	}

	private void DropSupplies(ThingDef thingDef, int count)
	{
		List<Thing> list = [];
		Thing thing = ThingMaker.MakeThing(thingDef);
		thing.stackCount = count;
		list.Add(thing);
		DropPodUtility.DropThingsNear(Data.siegeCenter, base.Map, list);
	}

	public override void Cleanup()
	{
		LordToilData_Siege lordToilData_Siege = Data;
		lordToilData_Siege.blueprints.RemoveAll((Blueprint blue) => blue.Destroyed);
		for (int i = 0; i < lordToilData_Siege.blueprints.Count; i++)
		{
			lordToilData_Siege.blueprints[i].Destroy(DestroyMode.Cancel);
		}
		foreach (Frame item in Frames.ToList())
		{
			item.Destroy(DestroyMode.Cancel);
		}
		foreach (Building ownedBuilding in lord.ownedBuildings)
		{
			ownedBuilding.SetFaction(null);
		}
	}
}
