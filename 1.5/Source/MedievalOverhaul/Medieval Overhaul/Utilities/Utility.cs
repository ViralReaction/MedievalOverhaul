using System;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class Utility
    {
        public static SeperateHideList WhiteList = DefDatabase<SeperateHideList>.GetNamed("WhiteList");
        public static SeperateWoodList LogList = DefDatabase<SeperateWoodList>.GetNamed("LogList");
        public static HideGraphicList HideGraphicList = DefDatabase<HideGraphicList>.GetNamed("HideGraphicList");
        public static StimulantDrugList StimulantDrugList = DefDatabase<StimulantDrugList>.GetNamed("StimulantDrugList");
        public static ModContentPack myContentPack = LoadedModManager.GetMod<MedievalOverhaulSettings>().Content;
        public static bool CEIsEnabled = LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId == "ceteam.combatextended"));
        public static bool VBEIsEnabled = ModsConfig.IsActive("VanillaExpanded.VBooksE");
        public static bool LWMFuelFilterIsEnabled = ModsConfig.IsActive("zal.lwmfuelfilter");
        public static RoomRoleDef DankPyon_Library;
        public static ThoughtDef DankPyon_ReadInLibrary;
       

        
        static Utility()
        {
            if (VBEIsEnabled is false)
            {
                DankPyon_Library = DefDatabase<RoomRoleDef>.GetNamed("DankPyon_Library");
                DankPyon_ReadInLibrary = DefDatabase<ThoughtDef>.GetNamed("DankPyon_ReadInLibrary");
            }
        }
        public static int GetFuelCountToFullyRefuel(CompRefuelable RefuelableComp)
        {
            if (RefuelableComp.Props.atomicFueling)
            {
                return Mathf.CeilToInt(RefuelableComp.Props.fuelCapacity / RefuelableComp.Props.FuelMultiplierCurrentDifficulty);
            }
            return Mathf.Max(Mathf.CeilToInt((RefuelableComp.TargetFuelLevel - RefuelableComp.fuel) / RefuelableComp.Props.FuelMultiplierCurrentDifficulty), 1);
        }
        public static int GetFuelCountToFullyRefuel(CompRefuelable RefuelableComp, Thing fuelThing)
        {
            float fuelValue = CachingUtility.FuelValueDict.GetValueOrDefault(fuelThing.def, 1f);
            return Mathf.CeilToInt(GetFuelCountToFullyRefuel(RefuelableComp) / fuelValue);
        }

        public static Thing FindSpecificFuel(Pawn pawn, ThingDef fuel)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(fuel), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: new Predicate<Thing>(validator));

            bool validator(Thing x) => !ForbidUtility.IsForbidden(x, pawn) && pawn.CanReserve((LocalTargetInfo)x);
        }

        public static Thing FindSpecificClosestFuel(Pawn pawn, List<ThingDef> fuelDefList)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: new Predicate<Thing>(validator));

            bool validator(Thing x) => !ForbidUtility.IsForbidden(x, pawn) && fuelDefList.Contains(x.def) && pawn.CanReserve((LocalTargetInfo)x);
        }
        public static bool FilterItemExists(ThingFilter filter, Pawn pawn)
        {
            foreach (var def in filter.AllowedThingDefs)
            {
                List<Thing> thingsOfDef = pawn.Map.listerThings.ThingsOfDef(def);
                if (thingsOfDef.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static string RemoveSubstring(ThingDef thingDef, string partToRemove)
        {
            string stringDefName = thingDef.defName;
            int index = stringDefName.IndexOf(partToRemove);
            if (index == -1)
            {
                return stringDefName;
            }
            else
            {
                string modifiedString = stringDefName.Replace(partToRemove, "");

                if (IsValidString(modifiedString))
                {
                    return modifiedString;
                }
                else
                {
                    return stringDefName;
                }
            }
        }
        static bool IsValidString(string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        public static Mineable_CompSpawnerDestroy GetFirstMineable(this IntVec3 c, Map map)
        {
            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] is Mineable_CompSpawnerDestroy mineable)
                {
                    return mineable;
                }
            }
            return null;
        }
        public static void GetUsableDeepDrills(Map map, List<Thing> outDrills)
        {
            outDrills.Clear();
            List<Thing> list = map.listerThings.ThingsOfDef(MedievalOverhaulDefOf.DankPyon_MineShaft);
            Faction ofPlayer = Faction.OfPlayer;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Faction == ofPlayer && list[i].TryGetComp<CompCreatesInfestations_Mine>().CanCreateInfestationNow)
                {
                    outDrills.Add(list[i]);
                }
            }
        }


        public static IEnumerable<Thing> GetOils(Pawn_InventoryTracker inventory)
        {
            foreach (Thing thing in inventory.innerContainer)
            {
                if (thing.TryGetComp<CompUseEffect_AddOil>() != null)
                {
                    yield return thing;
                }
            }
            yield break;

        public static Pawn FindBestHauler(Thing thing)
        {
            Map map = thing.Map;
            List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;

            Pawn bestHauler = null;
            float bestDistance = float.MaxValue;

            foreach (Pawn pawn in pawns)
            {
                if (!pawn.Drafted &&
                    pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                    !pawn.Downed &&
                    pawn.CanReserve(thing) &&
                    pawn.workSettings.WorkIsActive(WorkTypeDefOf.Hauling))
                {
                    float distance = pawn.Position.DistanceTo(thing.Position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestHauler = pawn;
                    }
                }
            }

            return bestHauler;

        }
    }
}
