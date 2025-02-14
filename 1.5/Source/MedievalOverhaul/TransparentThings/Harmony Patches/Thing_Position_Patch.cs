using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TransparentThings
{
    [HarmonyPatch(typeof(Thing), "Position", MethodType.Setter)]
    public class Thing_Position_Patch
    {
        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }
        private static Dictionary<Map, TreeTransparencyGrid> cachedTransparencyGrids = new Dictionary<Map, TreeTransparencyGrid>();
        public static void Postfix(Thing __instance, bool __state)
        {
            if (__instance is not Pawn pawn || __instance.Map == null)
                return;

            Map map = pawn.Map;

            // Retrieve or cache the map's TreeTransparencyGrid
            if (!cachedTransparencyGrids.TryGetValue(map, out TreeTransparencyGrid transparencyGrid))
            {
                transparencyGrid = map.GetComponent<TreeTransparencyGrid>();
                if (transparencyGrid != null)
                {
                    cachedTransparencyGrids[map] = transparencyGrid; // Cache the reference
                }
            }

            if (transparencyGrid == null) return; // Safety check

            // Skip if this position was already checked recently
            if (transparencyGrid.checkedPositions.Contains(pawn.Position)) return;

            transparencyGrid.checkedPositions.Add(pawn.Position);
            List<Plant> nearbyTrees = transparencyGrid.GetTreesNear(pawn.Position);

            foreach (Plant tree in nearbyTrees)
            {
                transparencyGrid.CheckAndApplyTransparency(tree, pawn);
            }
        }
    }

}
