using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace MedievalOverhaul
{
    public static class ThingFilterUI_Fuel
    {
        public static void DoThingFilterConfigWindow(Rect rect, ThingFilterUI_Fuel.UIState state, ThingFilter filter, List<ICompFuelHandler> _cachedFuelBuildings, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, bool forceHideHitPointsConfig = false, bool forceHideQualityConfig = false, bool showMentalBreakChanceRange = false, List<ThingDef> suppressSmallVolumeTags = null, Map map = null)
        {
            Widgets.DrawMenuSection(rect);
            float num = rect.width - 2f;
            Rect rect2 = new Rect(rect.x + 3f, rect.y + 3f, num / 2f - 3f - 1.5f, 24f);
            if (Widgets.ButtonText(rect2, "ClearAll".Translate(), true, true, true, null))
            {
                filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 3f, rect2.y, rect2.width, 24f), "AllowAll".Translate(), true, true, true, null))
            {
                filter.SetAllowAll(parentFilter, false);
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }
            rect.yMin = rect2.yMax;
            Rect rect3 = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 16f - 6f, 24f);
            state.quickSearch.OnGUI(rect3, null, null);
            rect.yMin = rect3.yMax + 3f;
            TreeNode_ThingCategory node = filter.RootNode;
            bool flag = true;
            bool flag2 = true;
            if (parentFilter != null)
            {
                node = parentFilter.DisplayRootCategory;
                flag = parentFilter.allowedHitPointsConfigurable;
                flag2 = parentFilter.allowedQualitiesConfigurable;
            }
            rect.xMax -= 4f;
            rect.yMax -= 6f;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, ThingFilterUI.viewHeight);
            Rect visibleRect = new Rect(0f, 0f, rect.width, rect.height);
            visibleRect.position += state.scrollPosition;
            Widgets.BeginScrollView(rect, ref state.scrollPosition, viewRect, true);
            float num2 = 2f;
            if (flag && !forceHideHitPointsConfig)
            {
                ThingFilterUI_Fuel.DrawHitPointsFilterConfig(ref num2, viewRect.width, filter);
            }
            if (flag2 && !forceHideQualityConfig)
            {
                ThingFilterUI_Fuel.DrawQualityFilterConfig(ref num2, viewRect.width, filter);
            }
            if (ModsConfig.AnomalyActive && showMentalBreakChanceRange)
            {
                ThingFilterUI_Fuel.DrawMentalBreakFilterConfig(ref num2, viewRect.width, filter);
            }
            float num3 = num2;
            Rect rect4 = new Rect(0f, num2, viewRect.width, 9999f);
            visibleRect.position -= rect4.position;
            Listing_TreeThingFilter_Fuel listing_TreeThingFilter = new Listing_TreeThingFilter_Fuel(filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags, state.quickSearch.filter, _cachedFuelBuildings);
            listing_TreeThingFilter.Begin(rect4);
            listing_TreeThingFilter.ListCategoryChildren(node, openMask, map, visibleRect);
            listing_TreeThingFilter.End();
            state.quickSearch.noResultsMatched = (listing_TreeThingFilter.matchCount == 0);
            if (Event.current.type == EventType.Layout)
            {
                ThingFilterUI.viewHeight = num3 + listing_TreeThingFilter.CurHeight + ExtraViewHeight;
            }
            Widgets.EndScrollView();
        }

        private static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            FloatRange allowedHitPointsPercents = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(rect, 1, ref allowedHitPointsPercents, 0f, 1f, "HitPoints", ToStringStyle.PercentZero, 0f, GameFont.Small, null);
            filter.AllowedHitPointsPercents = allowedHitPointsPercents;
            y += 32f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        private static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            QualityRange allowedQualityLevels = filter.AllowedQualityLevels;
            Widgets.QualityRange(rect, 876813230, ref allowedQualityLevels);
            filter.AllowedQualityLevels = allowedQualityLevels;
            y += 32f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        private static void DrawMentalBreakFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            FloatRange allowedMentalBreakChance = filter.AllowedMentalBreakChance;
            Widgets.FloatRange(rect, 968573221, ref allowedMentalBreakChance, 0f, 1f, "MaxMentalBreakChance", ToStringStyle.PercentZero, 0f, GameFont.Small, null);
            filter.AllowedMentalBreakChance = allowedMentalBreakChance;
            y += 32f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        private static float viewHeight;

        private const float ExtraViewHeight = 90f;

        private const float RangeLabelTab = 10f;

        private const float RangeLabelHeight = 19f;

        private const float SliderHeight = 32f;

        private const float SliderTab = 20f;

        public class UIState
        {
            public Vector2 scrollPosition;

            public QuickSearchWidget quickSearch = new QuickSearchWidget();
        }
    }
}