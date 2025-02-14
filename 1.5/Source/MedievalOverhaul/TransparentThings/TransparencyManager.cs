using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    public static class TreeTransparencyManager
    {
        private static Dictionary<Thing, Shader> originalShaders = new Dictionary<Thing, Shader>();
        public  static HashSet<Plant> transparentTrees = new HashSet<Plant>();
        private static Dictionary<Plant, float> lastSeenTransparentTime = new Dictionary<Plant, float>(); // Tracks when trees were last transparent

        private static float transparencyDuration = 5f;

        public static void CheckAndApplyTransparency(Plant tree, Thing pawn)
        {
            if (tree == null || pawn == null || tree == pawn || !tree.Spawned || !pawn.Spawned)
                return;

            TreeTransparencyGrid grid = tree.Map.GetComponent<TreeTransparencyGrid>();
            bool shouldBeTransparent = grid.IsPositionInsideTree(pawn.Position);

            if (shouldBeTransparent)
            {
                ApplyTransparency(tree);
            }
            else
            {
                RestoreOriginalShader(tree);
            }
        }

        private static void ApplyTransparency(Plant tree)
        {
            if (transparentTrees.Contains(tree)) return;

            if (!originalShaders.ContainsKey(tree))
            {
                originalShaders[tree] = tree.Graphic.Shader; // Store the original shader
            }

            Shader transparentShader = ShaderDatabase.Transparent;

            if (tree.Graphic.Shader != transparentShader)
            {
                
                tree.Map.mapDrawer.MapMeshDirty(tree.Position, MapMeshFlagDefOf.Things);
            }
            transparentTrees.Add(tree);
            lastSeenTransparentTime[tree] = Time.time;
        }

        private static void RestoreOriginalShader(Plant tree)
        {
            if (!transparentTrees.Contains(tree)) return;

            // Wait for the cooldown before restoring transparency
            if (Time.time - lastSeenTransparentTime[tree] < transparencyDuration)
                return;

            if (originalShaders.TryGetValue(tree, out Shader originalShader)) // Use TryGetValue to prevent KeyNotFoundException
            {
                //tree.Graphic = tree.Graphic.GetColoredVersion(originalShader, tree.Graphic.Color, tree.Graphic.ColorTwo);
                tree.Map.mapDrawer.MapMeshDirty(tree.Position, MapMeshFlagDefOf.Things);
                originalShaders.Remove(tree);
            }
            else
            {
                Log.Warning($"[TreeTransparency] Tried to restore shader for {tree.def.defName}, but it was not in originalShaders!"); // Debug warning
            }

            transparentTrees.Remove(tree); // Remove from cache
            lastSeenTransparentTime.Remove(tree);


            //if (originalShaders.TryGetValue(tree, out Shader originalShader))
            //{
            //    transparentTrees.Remove(tree);
            //    tree.Map.mapDrawer.MapMeshDirty(tree.Position, MapMeshFlagDefOf.Things);
            //    originalShaders.Remove(tree);
            //}
        }
    }

}