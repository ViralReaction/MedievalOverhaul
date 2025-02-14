using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static bool hasTransparentTrees;
        public static bool hasTransparentRoofs;
        public static List<ThingDef> transparentRoofs;
        public static Dictionary<Map, Dictionary<IntVec3, List<Thing>>> cachedTransparentableCellsByMaps = new Dictionary<Map, Dictionary<IntVec3, List<Thing>>>();
        public static Dictionary<Thing, ThingExtension> cachedTransparentableThingsByExtensions = new Dictionary<Thing, ThingExtension>();
        public static Dictionary<Map, List<ThingWithExtension>> cachedTransparentableThingsByMaps = new Dictionary<Map, List<ThingWithExtension>>();
        public static Dictionary<Thing, HashSet<IntVec3>> cachedCells = new Dictionary<Thing, HashSet<IntVec3>>();
        public static Dictionary<Thing, Shader> lastCachedShaders = new Dictionary<Thing, Shader>();
        public static List<Thing> transparentThings = new List<Thing>();
        public static Dictionary<Thing, bool> matchedItems = new Dictionary<Thing, bool>();

        static Core()
        {
            hasTransparentTrees = DefDatabase<ThingDef>.AllDefs.All<ThingDef>((Func<ThingDef, bool>)(x => x.IsPlant && HasTransparencyExtension(x)));
            transparentRoofs = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((Func<ThingDef, bool>)(x => typeof(RoofSetter).IsAssignableFrom(x.thingClass) && HasTransparencyExtension(x))).ToList<ThingDef>();
            hasTransparentRoofs = transparentRoofs.Any<ThingDef>();
            static bool HasTransparencyExtension(ThingDef x)
            {
                ThingExtension modExtension = x.GetModExtension<ThingExtension>();
                return modExtension != null && (modExtension.transparentWhenPawnIsInsideArea || modExtension.transparentWhenItemIsInsideArea);
            }
        }

        public static HashSet<IntVec3> GetTransparentCheckArea(this Thing thing, ThingExtension extension)
        {
            HashSet<IntVec3> transparentCheckAreaInt;
            if (!Core.cachedCells.TryGetValue(thing, out transparentCheckAreaInt))
                Core.cachedCells[thing] = transparentCheckAreaInt = Core.GetTransparentCheckAreaInt(thing, extension);
            return transparentCheckAreaInt;
        }

        public static HashSet<IntVec3> GetTransparentCheckAreaInt(Thing thing, ThingExtension extension)
        {
            //IntVec3 bottomLeft = thing.OccupiedRect().; // Probably wrong
            CellRect occupiedRect = thing.OccupiedRect();
            IntVec3 bottomLeft = new IntVec3(occupiedRect.minX, 0, occupiedRect.minZ);
            CellRect cellRect = new CellRect(bottomLeft.x, bottomLeft.z, extension.firstArea.x, extension.firstArea.z);
            if (extension.firstAreaOffset != IntVec2.Zero)
                cellRect = cellRect.MovedBy(extension.firstAreaOffset);
            //List<IntVec3> list = cellRect.Cells.ToList<IntVec3>();
            List<IntVec3> list = [];
            foreach (IntVec3 cell in cellRect.Cells)
            {
                list.Add(cell);
            }
            if (extension.secondArea != IntVec2.Zero)
            {
                CellRect collection = new CellRect(bottomLeft.x, bottomLeft.z, extension.secondArea.x, extension.secondArea.z);
                if (extension.secondAreaOffset != IntVec2.Zero)
                    collection = collection.MovedBy(extension.secondAreaOffset);
                list.AddRange((IEnumerable<IntVec3>)collection);
            }
            //return list.Where<IntVec3>((Func<IntVec3, bool>)(x => x.InBounds(thing.Map))).ToHashSet<IntVec3>();
            HashSet<IntVec3> result = new HashSet<IntVec3>();
            foreach (IntVec3 cell in list)
            {
                if (cell.InBounds(thing.Map))
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        public static bool HasItemsInCell(IntVec3 cell, Map map, ThingExtension extension)
        {
            foreach (Thing thing in cell.GetThingList(map))
            {
                if (Core.ItemMatches(thing, extension))
                    return true;
            }
            return false;
        }

        public static bool BaseItemMatches(Thing thing) => thing is Pawn || thing.def.category == ThingCategory.Item;

        public static bool ItemMatches(Thing thing, ThingExtension extension)
        {
            bool flag;
            if (!Core.matchedItems.TryGetValue(thing, out flag))
                Core.matchedItems[thing] = flag = (thing is Pawn && extension.transparentWhenPawnIsInsideArea || thing.def.category == ThingCategory.Item && extension.transparentWhenItemIsInsideArea) && (extension.ignoredThings == null || !extension.ignoredThings.Contains(thing.def));
            return flag;
        }

        public static void RecheckTransparency(Thing thing, Thing otherThing, ThingExtension extension)
        {
            // Early exit if nothing needs to be checked
            if (thing == otherThing || !thing.Spawned || thing.Map != otherThing.Map)
                return;
            Shader shader;
            if (!Core.lastCachedShaders.TryGetValue(thing, out shader))
                Core.lastCachedShaders[thing] = shader = thing.Graphic.Shader;
            bool flag = (UnityEngine.Object)shader == (UnityEngine.Object)thing.GetTransparencyShader();
            if (!flag && Core.ItemMatches(otherThing, extension) && thing.GetTransparentCheckArea(extension).Contains(otherThing.Position))
            {
                thing.Map.mapDrawer.MapMeshDirty(thing.Position, 1);
                if (!Core.transparentThings.Contains(thing))
                    Core.transparentThings.Add(thing);
            }
            if (flag)
            {
                HashSet<IntVec3> transparentCheckArea = thing.GetTransparentCheckArea(extension);
                bool hasItems = false;
                foreach (IntVec3 cell in transparentCheckArea)
                {
                    if (Core.HasItemsInCell(cell, thing.Map, extension))
                    {
                        hasItems = true;
                        break;
                    }
                }
                if (!transparentCheckArea.Contains(otherThing.Position) && !hasItems)
                {
                    thing.Map.mapDrawer.MapMeshDirty(thing.Position, 1);
                    Core.transparentThings.Remove(thing);
                }

            }
        }

        public static Shader GetTransparencyShader(this Thing thing) => thing is Plant ? TransparentThingsDefOf.TransparentPlant.Shader : TransparentThingsDefOf.TransparentPostLight.Shader;

        public static void FormTransparencyGridIn(Map map)
        {
            List<ThingWithExtension> thingWithExtensionList;
            if (!Core.cachedTransparentableThingsByMaps.TryGetValue(map, out thingWithExtensionList))
                return;
            Dictionary<IntVec3, List<Thing>> dictionary;
            if (Core.cachedTransparentableCellsByMaps.TryGetValue(map, out dictionary))
                dictionary.Clear();
            else
                Core.cachedTransparentableCellsByMaps[map] = dictionary = new Dictionary<IntVec3, List<Thing>>();
            foreach (ThingWithExtension thingWithExtension in thingWithExtensionList)
            {
                foreach (IntVec3 key in thingWithExtension.thing.GetTransparentCheckArea(thingWithExtension.extension))
                {
                    if (dictionary.ContainsKey(key))
                        dictionary[key].Add(thingWithExtension.thing);
                    else
                        dictionary[key] = new List<Thing>()
            {
              thingWithExtension.thing
            };
                }
            }
        }

        public static bool TransparencyAllowed(this Thing thing)
        {
            switch (thing)
            {
                case Plant _:
                    return TransparentThingsMod.settings.enableTreeTransparency;
                case RoofSetter _:
                    return TransparentThingsMod.settings.enableRoofTransparency;
                default:
                    return false;
            }
        }

    }
}
