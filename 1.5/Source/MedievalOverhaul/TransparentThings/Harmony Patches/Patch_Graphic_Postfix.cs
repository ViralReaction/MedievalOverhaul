using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;
using MedievalOverhaul;

namespace TransparentThings
{
    [HarmonyPatch(typeof(Plant), nameof(Plant.Graphic), MethodType.Getter)]
    public static class Patch_Graphic_Postfix
    {
        private static Dictionary<Map, TreeTransparencyGrid> cachedTransparencyGrids = new();
        private static Dictionary<Graphic, Graphic> transparentGraphicCache = new();
        private static HashSet<Plant> alreadyTransparentTrees = new(); // Tracks trees that already have transparency applied

        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }

        private static void Postfix(Plant __instance, ref Graphic __result)
        {
            if (__instance.Map == null) return;

            // Retrieve cached transparency grid or fetch it
            if (!cachedTransparencyGrids.TryGetValue(__instance.Map, out TreeTransparencyGrid transparencyGrid))
            {
                transparencyGrid = __instance.Map.GetComponent<TreeTransparencyGrid>();
                if (transparencyGrid != null)
                {
                    cachedTransparencyGrids[__instance.Map] = transparencyGrid; // Cache it
                }
            }

            if (transparencyGrid == null) return;
            if (transparencyGrid.transparentTrees.Contains(__instance))
            {
                // Ensure the tree is actually transparent
                Graphic newGraphic = GetTransparentGraphic(__instance, __result);

                if (__result != newGraphic) // Only update if different
                {
                    __result = newGraphic;
                    alreadyTransparentTrees.Add(__instance); // Mark as processed
                    __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlagDefOf.Things);
                }
            }
            else
            {
                // Remove tree from tracking if it is no longer transparent
                alreadyTransparentTrees.Remove(__instance);
            }
        }

        public static Graphic GetTransparentGraphic(Plant __instance, Graphic initialGraphic)
        {
            if (!__instance.def.plant?.IsTree ?? true || __instance.Map == null)
                return initialGraphic;
            if (__instance.def.GetModExtension<ThingExtension>() == null) return initialGraphic;

            // Check if we already generated a transparent graphic for this one
            if (transparentGraphicCache.TryGetValue(initialGraphic, out Graphic cachedGraphic))
            {
                return cachedGraphic;
            }

            // Get transparency shader
            Shader transparencyShader = ShaderDatabase.TransparentPlant;
            // Create the new graphic
            Graphic newGraphic = GraphicDatabase.Get(initialGraphic.GetType(), "Transparent/" + initialGraphic.path, transparencyShader, __instance.def.graphicData.drawSize, initialGraphic.color, initialGraphic.colorTwo);

            // Cache the transparent version
            transparentGraphicCache[initialGraphic] = newGraphic;

            return newGraphic;
        }

        // Clears cached transparency grids for removed maps
        public static void ClearMapCache(Map map)
        {
            cachedTransparencyGrids.Remove(map);
        }

        // Clears graphic cache periodically to prevent memory bloat
        public static void ClearGraphicCache()
        {
            transparentGraphicCache.Clear();
            alreadyTransparentTrees.Clear();
        }
    }

}