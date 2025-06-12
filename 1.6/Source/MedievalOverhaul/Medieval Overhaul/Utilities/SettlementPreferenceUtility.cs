using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MedievalOverhaul
{
    internal class SettlementPreferenceUtility
    {
        public static bool GetTileID(Faction faction, out int tileID)
        {
            tileID = 0;
            bool defaultToVanilla = false;
            SettlementPreference modExt = SettlementPreference.Get((Def)faction.def);
            int countMaxIterations = MedievalOverhaulSettings.settings.StandaloneSettlementPreference_Iterations;
            bool flag = MedievalOverhaulSettings.settings.StandaloneSettlementPreference_Logging;
            bool flagExtra = MedievalOverhaulSettings.settings.StandaloneSettlementPreference_LoggingExtended;
            for (int index = 0; index < countMaxIterations; ++index)
            {
                if (Enumerable.Range(0, 100).Select<int, int>((Func<int, int>)(_ => Rand.Range(0, Find.WorldGrid.TilesCount))).TryRandomElementByWeight<int>((Func<int, float>)(x =>
                {
                    defaultToVanilla = false;
                    Tile tile = Find.WorldGrid[x];
                    string str = "Faction: " + faction?.ToString() + ", checking tile: " + tile?.ToString() + ", ";
                    if (tile is not SurfaceTile surfaceTile || !surfaceTile.biome.canBuildBase || !surfaceTile.biome.implemented)
                        return 0.0f;
                    if (!modExt.biomeKeyWords.NullOrEmpty<string>() && !modExt.biomeKeyWords.Any<string>((Predicate<string>)(y => surfaceTile.biome.defName.Contains(y))))
                    {
                        if (flag)
                            str += "failed biomeKeyWords, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.useTemperatureRange && !SettlementPreferenceUtility.CheckTemperatureRange(modExt.temperatureRangeMin, modExt.temperatureRangeMax, x))
                    {
                        if (flag)
                            str += "failed CheckTemperatureRange, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.useElevationRange && !SettlementPreferenceUtility.CheckElevationRange(modExt.elevationRangeMin, modExt.elevationRangeMax, surfaceTile))
                    {
                        if (flag)
                            str += "failed CheckAltitudeRange, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.useSwampinessRange && !SettlementPreferenceUtility.CheckSwampinessRange(modExt.swampinessRangeMin, modExt.swampinessRangeMax, surfaceTile))
                    {
                        if (flag)
                            str += "failed CheckSwampinessRange, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.useSwampinessRange && !SettlementPreferenceUtility.CheckRainfallRange(modExt.rainfallRangeMin, modExt.rainfallRangeMax, surfaceTile))
                    {
                        if (flag)
                            str += "failed CheckRainfallRange, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.likedBiomeList != null && !modExt.likedBiomeList.Contains(surfaceTile.biome.defName))
                    {
                        if (flag)
                            str += "failed likedBiomeList, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.dislikedBiomeList != null && modExt.dislikedBiomeList.Contains(surfaceTile.biome.defName))
                    {
                        if (flag)
                            str += "failed dislikedBiomeList, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.requiredHillLevels != null && !modExt.requiredHillLevels.Contains(surfaceTile.hilliness))
                    {
                        if (flag)
                            str += "failed requiredHillLevels, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.disallowedHillLevels != null && modExt.disallowedHillLevels.Contains(surfaceTile.hilliness))
                    {
                        if (flag)
                            str += "failed disallowedHillLevels, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.requiredHillLevels == null && modExt.disallowedHillLevels == null && surfaceTile.hilliness == Hilliness.Impassable)
                    {
                        if (flag)
                            str += "failed impassible hills check, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.requiresWater && !SettlementPreferenceUtility.CheckTileBiomeNeighbours(x, BiomeDefOf.Ocean) && !SettlementPreferenceUtility.CheckTileBiomeNeighbours(x, BiomeDefOf.Lake) && surfaceTile.Rivers == null)
                    {
                        if (flag)
                            str += "failed requiresWater, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.onlyCoastal && !SettlementPreferenceUtility.CheckTileBiomeNeighbours(x, BiomeDefOf.Ocean))
                    {
                        if (flag)
                            str += "failed onlyCoastal, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.onlyLakeside && !SettlementPreferenceUtility.CheckTileBiomeNeighbours(x, BiomeDefOf.Lake))
                    {
                        if (flag)
                            str += "failed onlyLakeside, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.onlyRiver && surfaceTile.Rivers == null)
                    {
                        if (flag)
                            str += "failed onlyRiver, ";
                        defaultToVanilla = true;
                    }
                    if (modExt.onlyRoad && surfaceTile.Roads == null)
                    {
                        if (flag)
                            str += "failed onlyRoad, ";
                        defaultToVanilla = true;
                    }
                    if (flag && !defaultToVanilla)
                        Log.Message(str + " valid tile = " + (!defaultToVanilla).ToString());
                    if (flagExtra & defaultToVanilla)
                        Log.Message(str + " valid tile = " + (!defaultToVanilla).ToString());
                    if (!defaultToVanilla && !modExt.IgnoreBiomeSelectionWeight)
                    {
                        float settlementSelectionWeight = tile.biome.settlementSelectionWeight;
                        if (faction?.def.minSettlementTemperatureChanceCurve != null)
                            settlementSelectionWeight *= faction.def.minSettlementTemperatureChanceCurve.Evaluate(GenTemperature.MinTemperatureAtTile(x));
                        return settlementSelectionWeight;
                    }
                    return !defaultToVanilla ? 1000f : 0.0f;
                }), out int result) && !defaultToVanilla && SettlementPreferenceUtility.FinalCheckTileIsValid(result))
                {
                    if (flag)
                        Log.Message("Faction: " + faction?.ToString() + ", passed all checks for tile: " + Find.WorldGrid[result]?.ToString());
                    tileID = result;
                    return false;
                }
            }
            if (flag)
                Log.Error("Failed to find faction base tile for " + faction?.ToString() + ", using ESCP_RaceTools.SettlementPreference. Defaulting to standard selection.");
            return true;
        }

        public static bool FinalCheckTileIsValid(int tile, StringBuilder reason = null)
        {
            Tile tile1 = Find.WorldGrid[tile];
            if (!tile1.biome.canBuildBase)
            {
                reason?.Append((string)"CannotLandBiome".Translate((NamedArgument)tile1.biome.LabelCap));
                return false;
            }
            if (!tile1.biome.implemented)
            {
                reason?.Append((string)("BiomeNotImplemented".Translate() + ": " + tile1.biome.LabelCap));
                return false;
            }
            Settlement settlement = Find.WorldObjects.SettlementBaseAt(tile);
            if (settlement != null)
            {
                if (reason != null)
                {
                    if (settlement.Faction == null)
                        reason.Append((string)"TileOccupied".Translate());
                    else if (settlement.Faction == Faction.OfPlayer)
                        reason.Append((string)"YourBaseAlreadyThere".Translate());
                    else
                        reason.Append((string)"BaseAlreadyThere".Translate((NamedArgument)settlement.Faction.Name));
                }
                return false;
            }
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
            {
                reason?.Append((string)"FactionBaseAdjacent".Translate());
                return false;
            }
            if (!Find.WorldObjects.AnyMapParentAt(tile) && Current.Game.FindMap(tile) == null && !Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
                return true;
            reason?.Append((string)"TileOccupied".Translate());
            return false;
        }

        public static bool CheckTemperatureRange(float min, float max, int tile) => (double)min <= (double)GenTemperature.MinTemperatureAtTile(tile) && (double)max >= (double)GenTemperature.MaxTemperatureAtTile(tile);

        public static bool CheckElevationRange(float min, float max, Tile tile) => (double)min <= (double)tile.elevation && (double)max >= (double)tile.elevation;

        public static bool CheckSwampinessRange(float min, float max, Tile tile) => (double)min <= (double)tile.swampiness && (double)max >= (double)tile.swampiness;

        public static bool CheckRainfallRange(float min, float max, Tile tile) => (double)min <= (double)tile.rainfall && (double)max >= (double)tile.rainfall;

        public static bool CheckTileBiomeNeighbours(PlanetTile tile, BiomeDef b)
        {
            List<PlanetTile> outNeighbors = new List<PlanetTile>();
            Find.WorldGrid.GetTileNeighbors(tile, outNeighbors);
            foreach (PlanetTile tileID in outNeighbors)
            {
                if (Find.WorldGrid[tileID].biome == b)
                    return true;
            }
            return false;
        }
    }
}
