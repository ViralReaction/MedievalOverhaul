﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ESCP_FuelExtension
{
    [HarmonyPatch(typeof(RefuelWorkGiverUtility))]
    [HarmonyPatch("FindBestFuel")]
    public class FindBestFuelPatch
    {
        [HarmonyPrefix]
        public static bool FindBestFuel_Prefix(Pawn pawn, Thing refuelable, ref Thing __result)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled || !(refuelable is Building))
            {
                return true;
            }
            CompRefuelable compRefuelable = refuelable.TryGetComp<CompRefuelable>();
            CompStoreFuelThing compStorage = refuelable.TryGetComp<CompStoreFuelThing>();
            if (compRefuelable != null && compStorage != null)
            {
                ThingFilter filter = refuelable.TryGetComp<CompStoreFuelThing>().AllowedFuelFilter;
                if (FilterItemExists(filter, pawn))
                {
                    Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
                    IEnumerable<Thing> searchSet = pawn.Map.listerThings.ThingsMatchingFilter(filter);
                    __result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 9999f, validator, searchSet, 0, -1, false, RegionType.Set_Passable, false);
                }
                return false;
            }
            return true;
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
    }
}