using System.Collections.Generic;
using Verse;
using UnityEngine;
using RimWorld;

namespace TransparentThings
{


    public class TreeTransparencyGrid : MapComponent
    {
        private Dictionary<IntVec3, List<Plant>> treeGrid;
        public HashSet<Plant> transparentTrees = new();
        private HashSet<Plant> permanentlyTransparentTrees = new();
        private Dictionary<Thing, Shader> originalShaders = new();
        private static Dictionary<Map, TreeTransparencyGrid> cachedTransparencyGrids = new();
        public HashSet<IntVec3> checkedPositions = new();

        const int TICKINTERVAL = 600; // Clear every 10 seconds
        private int tickCounter = 0;

        public TreeTransparencyGrid(Map map) : base(map)
        {
            treeGrid = new Dictionary<IntVec3, List<Plant>>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RebuildGrid();
        }
        public override void MapComponentTick()
        {
            tickCounter++;
            if (tickCounter >= TICKINTERVAL)
            {
                checkedPositions.Clear();
                tickCounter = 0;
            }
            //CheckAndRestoreTransparency();
        }

        public void RebuildGrid()
        {
            treeGrid.Clear();
            foreach (Thing tree in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant))
            {
                if (tree is Plant plant && plant.def.plant.IsTree)
                {
                    AddTree(plant);
                }
            }
        }


        public void AddTree(Plant tree)
        {
            if (tree == null || !tree.Spawned)
                return;

            HashSet<IntVec3> coveredCells = GetTreeVisualArea(tree);

            foreach (IntVec3 cell in coveredCells)
            {
                if (!treeGrid.TryGetValue(cell, out var list))
                {
                    list = new List<Plant>();
                    treeGrid[cell] = list;
                }
                list.Add(tree);
            }
        }

        public void RemoveTree(Plant tree)
        {
            if (tree == null)
                return;

            HashSet<IntVec3> coveredCells = GetTreeVisualArea(tree);

            foreach (IntVec3 cell in coveredCells)
            {
                if (treeGrid.TryGetValue(cell, out var list))
                {
                    list.Remove(tree);
                    if (list.Count == 0)
                    {
                        treeGrid.Remove(cell);
                    }
                }
            }
        }

        public List<Plant> GetTreesNear(IntVec3 position, int radius = 2)
        {
            List<Plant> nearbyTrees = new List<Plant>();
            IntVec3 checkPos;

            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    checkPos = new IntVec3(position.x + x, 0, position.z + z);
                    if (treeGrid.TryGetValue(checkPos, out var trees))
                    {
                        nearbyTrees.AddRange(trees);
                    }
                }
            }

            return nearbyTrees;
        }

        public bool IsPositionInsideTree(IntVec3 position)
        {
            return treeGrid.ContainsKey(position);
        }

        public void CheckAndApplyTransparency(Plant tree, Pawn pawn)
        {
            if (tree == null || pawn == null || !tree.Spawned || !pawn.Spawned)
                return;

            if (permanentlyTransparentTrees.Contains(tree)) return;

            bool shouldBeTransparent = IsPositionInsideTree(pawn.Position);

            if (shouldBeTransparent)
            {
                ApplyTransparency(tree);
                permanentlyTransparentTrees.Add(tree); // 🔹 Mark as permanently transparent
            }
            //else
            //{
            //    RestoreOriginalShader(tree);
            //}
        }

        private void ApplyTransparency(Plant tree)
        {
            if (transparentTrees.Contains(tree)) return; // Skip if already transparent

            if (!originalShaders.ContainsKey(tree))
            {
                originalShaders[tree] = tree.Graphic.Shader; // Store original shader
            }

            Shader transparentShader = ShaderDatabase.Transparent;

            if (tree.Graphic.Shader != transparentShader)
            {
                map.mapDrawer.MapMeshDirty(tree.Position, MapMeshFlagDefOf.Things);
            }

            transparentTrees.Add(tree);
        }

        public void RestoreOriginalShader(Plant tree)
        {
            if (!transparentTrees.Contains(tree)) return; // Skip if not transparent

            if (originalShaders.TryGetValue(tree, out Shader originalShader))
            {
                map.mapDrawer.MapMeshDirty(tree.Position, MapMeshFlagDefOf.Things);
                originalShaders.Remove(tree);
            }

            transparentTrees.Remove(tree);
        }

        

        //private void CheckAndRestoreTransparency()
        //{
        //    List<Plant> toRestore = new List<Plant>();

        //    foreach (Plant tree in transparentTrees)
        //    {
        //        if (Time.time - lastSeenTransparentTime[tree] >= transparencyDuration)
        //        {
        //            toRestore.Add(tree);
        //        }
        //    }

        //    foreach (Plant tree in toRestore)
        //    {
        //        RestoreOriginalShader(tree);
        //    }
        //}

        private HashSet<IntVec3> GetTreeVisualArea(Thing tree)
        {
            HashSet<IntVec3> coveredCells = new HashSet<IntVec3>();

            Vector3 treeWorldPos = tree.DrawPos;
            Vector2 treeSize = tree.Graphic.drawSize;

            int halfWidth = Mathf.CeilToInt(treeSize.x / 2);
            int halfHeight = Mathf.CeilToInt(treeSize.y / 2);

            IntVec3 treeTile = tree.Position;

            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int z = -halfHeight; z <= halfHeight; z++)
                {
                    IntVec3 tile = new IntVec3(treeTile.x + x, 0, treeTile.z + z);
                    coveredCells.Add(tile);
                }
            }

            return coveredCells;
        }
    }
}