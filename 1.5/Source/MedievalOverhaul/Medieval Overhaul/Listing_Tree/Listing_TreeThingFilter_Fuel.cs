using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Listing_TreeThingFilter_Fuel : Listing_Tree
    {
        private static readonly Color NoMatchColor = Color.grey;
        private static readonly LRUCache<(TreeNode_ThingCategory, ThingFilter), List<SpecialThingFilterDef>> cachedHiddenSpecialFilters =
            new(500);

        private ThingFilter filter;
        private ThingFilter parentFilter;
        private List<SpecialThingFilterDef> hiddenSpecialFilters;
        private List<ThingDef> forceHiddenDefs;
        private List<SpecialThingFilterDef> tempForceHiddenSpecialFilters;
        private List<ThingDef> suppressSmallVolumeTags;
        //private List<CompStoreFuelThing> _cachedFuelBuildings;
        private List<ICompFuelHandler> _cachedFuelBuildings;
        protected QuickSearchFilter searchFilter;
        public int matchCount;
        private Rect visibleRect;

        public Listing_TreeThingFilter_Fuel(ThingFilter filter, ThingFilter parentFilter,
            IEnumerable<ThingDef> forceHiddenDefs, IEnumerable<SpecialThingFilterDef> forceHiddenFilters,
            List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter, List<ICompFuelHandler> cachedFuelBuildings)
        {
            this.filter = filter;
            this.parentFilter = parentFilter;
            this.forceHiddenDefs = forceHiddenDefs?.ToList();
            this.tempForceHiddenSpecialFilters = forceHiddenFilters?.ToList();
            this.suppressSmallVolumeTags = suppressSmallVolumeTags;
            this.searchFilter = searchFilter;
            this._cachedFuelBuildings = cachedFuelBuildings;
        }

        public void ListCategoryChildren(TreeNode_ThingCategory node, int openMask, Map map, Rect visibleRect)
        {
            this.visibleRect = visibleRect;
            int num = 0;

            foreach (var specialThingFilterDef in node.catDef.ParentsSpecialThingFilterDefs)
            {
                if (Visible(specialThingFilterDef, node))
                {
                    DoSpecialFilter(specialThingFilterDef, num);
                }
            }

            DoCategoryChildren(node, num, openMask, map, false);
        }

        private void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map,
            bool subtreeMatchedSearch)
        {
            var childSpecialFilters = node.catDef.childSpecialFilters;
            foreach (var filter in childSpecialFilters)
            {
                if (Visible(filter, node))
                {
                    DoSpecialFilter(filter, indentLevel);
                }
            }

            foreach (var childNode in node.ChildCategoryNodes)
            {
                if (Visible(childNode) && !HideCategoryDueToSearch(childNode, subtreeMatchedSearch))
                {
                    DoCategory(childNode, indentLevel, openMask, map, subtreeMatchedSearch);
                }
            }

            var undiscovered = new List<ThingDef>();
            foreach (var thingDef in node.catDef.SortedChildThingDefs)
            {
                if (Find.HiddenItemsManager.Hidden(thingDef))
                {
                    undiscovered.Add(thingDef);
                }
                else if (Visible(thingDef) && !HideThingDueToSearch(thingDef, subtreeMatchedSearch))
                {
                    DoThingDef(thingDef, indentLevel, map);
                }
            }

            if (!searchFilter.Active && undiscovered.Count > 0)
            {
                DoUndiscoveredEntry(indentLevel, node.catDef.parent != ThingCategoryDefOf.Corpses, undiscovered);
            }
        }

        private void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel)
        {
            if (!sfDef.configurable) return;

            Color? textColor = searchFilter.Matches(sfDef) ? null : NoMatchColor;

            if (CurrentRowVisibleOnScreen())
            {
                LabelLeft("*" + sfDef.LabelCap, sfDef.description, nestLevel, textColor: textColor);
                bool isAllowed = filter.Allows(sfDef);
                bool previousState = isAllowed;

                Widgets.Checkbox(new Vector2(LabelWidth, curY), ref isAllowed, lineHeight);

                if (isAllowed != previousState)
                {
                    filter.SetAllow(sfDef, isAllowed);

                    // Apply the change to all fuel storage buildings
                    foreach (var fuelBuilding in _cachedFuelBuildings)
                    {
                        fuelBuilding.AllowedFuelFilter.SetAllow(sfDef, isAllowed);
                    }
                }
            }

            EndLine();
        }


        private void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map,
            bool subtreeMatchedSearch)
        {
            Color? textColor = searchFilter.Active && !CategoryMatches(node) ? NoMatchColor : null;

            if (CurrentRowVisibleOnScreen())
            {
                OpenCloseWidget(node, indentLevel, openMask);
                LabelLeft(node.LabelCap, node.catDef.description, indentLevel, textColor: textColor);

                var checkboxState = AllowanceStateOf(node);
                var newCheckboxState =
                    Widgets.CheckboxMulti(new Rect(LabelWidth, curY, lineHeight, lineHeight), checkboxState);

                if (checkboxState != newCheckboxState)
                {
                    filter.SetAllow(node.catDef, newCheckboxState == MultiCheckboxState.On, forceHiddenDefs,
                        hiddenSpecialFilters);
                    foreach (var fuelBuilding in _cachedFuelBuildings)
                    {
                        fuelBuilding.AllowedFuelFilter.SetAllow(node.catDef, newCheckboxState == MultiCheckboxState.On, forceHiddenDefs,
                        hiddenSpecialFilters);
                    }
                }
            }

            EndLine();

            if (IsOpen(node, openMask))
            {
                DoCategoryChildren(node, indentLevel + 1, openMask, map, subtreeMatchedSearch);
            }
        }

        private void DoThingDef(ThingDef tDef, int nestLevel, Map map)
        {
            Color? color = searchFilter.Matches(tDef) ? null : NoMatchColor;


            if (tDef.uiIcon != null && tDef.uiIcon != BaseContent.BadTex)
            {
                nestLevel++;
                Widgets.DefIcon(new Rect(base.XAtIndentLevel(nestLevel) - 6f, this.curY, 20f, 20f), tDef, null, 1f, null, true, color, null, null);
            }
            if (CurrentRowVisibleOnScreen())
            {
                LabelLeft(tDef.LabelCap, tDef.DescriptionDetailed, nestLevel, textColor: color);

                // Determine current checkbox state
                MultiCheckboxState state = GetMultiCheckboxState(tDef);

                // Draw a multi-state checkbox
                Rect checkboxRect = new Rect(LabelWidth, curY, lineHeight, lineHeight);
                MultiCheckboxState newState = Widgets.CheckboxMulti(checkboxRect, state);

                if (newState != state)
                {
                    bool newAllowedState = newState == MultiCheckboxState.On;
                    // Update the main filter
                    filter.SetAllow(tDef, newAllowedState);

                    // Apply changes to all cached fuel storage buildings
                    foreach (var fuelBuilding in _cachedFuelBuildings)
                    {
                        fuelBuilding.AllowedFuelFilter.SetAllow(tDef, newAllowedState);
                    }
                }
            }
            EndLine();
        }

        private bool HideCategoryDueToSearch(TreeNode_ThingCategory subCat, bool subtreeMatchedSearch)
        {
            return searchFilter.Active && !subtreeMatchedSearch && !CategoryMatches(subCat) &&
                   !ThisOrDescendantsVisibleAndMatchesSearch(subCat);
        }

        private bool HideThingDueToSearch(ThingDef tDef, bool subtreeMatchedSearch)
        {
            return searchFilter.Active && !subtreeMatchedSearch && !searchFilter.Matches(tDef);
        }

        private bool CategoryMatches(TreeNode_ThingCategory node)
        {
            return searchFilter.Matches(node.catDef.label);
        }

        private bool Visible(TreeNode_ThingCategory node)
        {
            return node.catDef.DescendantThingDefs.Any(Visible);
        }

        private bool Visible(ThingDef td)
        {
            return td.PlayerAcquirable && (parentFilter?.Allows(td) ?? true);
        }

        private bool Visible(SpecialThingFilterDef f, TreeNode_ThingCategory node)
        {
            if (parentFilter != null && !parentFilter.Allows(f)) return false;

            hiddenSpecialFilters ??= GetCachedHiddenSpecialFilters(node, parentFilter);

            return !hiddenSpecialFilters.Contains(f);
        }

        private bool CurrentRowVisibleOnScreen()
        {
            var rowRect = new Rect(0f, curY, ColumnWidth, lineHeight);
            return visibleRect.Overlaps(rowRect);
        }

        private static List<SpecialThingFilterDef> GetCachedHiddenSpecialFilters(TreeNode_ThingCategory node,
            ThingFilter parentFilter)
        {
            var key = (node, parentFilter);
            if (!cachedHiddenSpecialFilters.TryGetValue(key, out var list))
            {
                list = CalculateHiddenSpecialFilters(node, parentFilter);
                cachedHiddenSpecialFilters.Add(key, list); // ✅ Use Add() instead of indexing
            }


            return list;
        }

        private static List<SpecialThingFilterDef> CalculateHiddenSpecialFilters(TreeNode_ThingCategory node,
            ThingFilter parentFilter)
        {
            var list = new List<SpecialThingFilterDef>();

            foreach (var specialFilter in node.catDef.DescendantSpecialThingFilterDefs)
            {
                if (parentFilter == null || parentFilter.Allows(specialFilter))
                {
                    list.Add(specialFilter);
                }
            }

            return list;
        }

        public static void ResetStaticData()
        {
            cachedHiddenSpecialFilters.Clear();
        }
        private void DoUndiscoveredEntry(int nestLevel, bool useIconOffset, List<ThingDef> toggledThingDefs)
        {
            if (!CurrentRowVisibleOnScreen()) return;

            var label = "UndiscoveredItemLabel".Translate();
            var tooltip = "UndiscoveredItemDesc".Translate().Resolve();

            LabelLeft(label, tooltip, nestLevel, 0f, null, useIconOffset ? 10f : 0f);

            bool isAllowed = filter.Allows(toggledThingDefs[0]);
            Widgets.Checkbox(new Vector2(LabelWidth, curY), ref isAllowed, lineHeight);

            if (isAllowed != filter.Allows(toggledThingDefs[0]))
            {
                foreach (var thingDef in toggledThingDefs)
                {
                    filter.SetAllow(thingDef, isAllowed);
                }
            }

            EndLine();
        }
        public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat)
        {
            int totalVisibleDefs = 0;
            int allowedDefs = 0;
            int totalVisibleFilters = 0;
            int allowedFilters = 0;

            // Count the number of visible and allowed ThingDefs
            foreach (ThingDef thingDef in cat.catDef.DescendantThingDefs)
            {
                if (Visible(thingDef))
                {
                    totalVisibleDefs++;
                    if (filter.Allows(thingDef))
                    {
                        allowedDefs++;
                    }
                }
            }

            // Count the number of visible and allowed SpecialThingFilterDefs
            foreach (SpecialThingFilterDef specialThingFilterDef in cat.catDef.DescendantSpecialThingFilterDefs)
            {
                if (Visible(specialThingFilterDef, cat))
                {
                    totalVisibleFilters++;
                    if (filter.Allows(specialThingFilterDef))
                    {
                        allowedFilters++;
                    }
                }
            }

            // If only special filters are relevant, check their state
            if (filter.OnlySpecialFilters)
            {
                if (allowedFilters == 0)
                {
                    return MultiCheckboxState.Off;
                }
                if (allowedFilters < totalVisibleFilters)
                {
                    return MultiCheckboxState.Partial;
                }
                return MultiCheckboxState.On;
            }

            // General case: check both ThingDefs and SpecialThingFilters
            if (allowedDefs == 0)
            {
                return MultiCheckboxState.Off;
            }
            if (totalVisibleDefs == allowedDefs && totalVisibleFilters == allowedFilters)
            {
                return MultiCheckboxState.On;
            }
            return MultiCheckboxState.Partial;
        }
        private bool ThisOrDescendantsVisibleAndMatchesSearch(TreeNode_ThingCategory node)
        {
            // Check if the current category is visible and matches the search filter
            if (Visible(node) && CategoryMatches(node))
            {
                return true;
            }

            // Check if any child ThingDef matches the search filter and is visible
            foreach (ThingDef td in node.catDef.childThingDefs)
            {
                if (ThingDefVisibleAndMatches(td))
                {
                    return true;
                }
            }

            // Check if any child SpecialThingFilterDef matches the search filter and is visible
            foreach (SpecialThingFilterDef sf in node.catDef.childSpecialFilters)
            {
                if (SpecialFilterVisibleAndMatches(sf, node))
                {
                    return true;
                }
            }

            // Recursively check child categories
            foreach (ThingCategoryDef subCategory in node.catDef.childCategories)
            {
                if (ThisOrDescendantsVisibleAndMatchesSearch(subCategory.treeNode))
                {
                    return true;
                }
            }

            return false;
        }

        // Helper method for ThingDefs
        private bool ThingDefVisibleAndMatches(ThingDef td)
        {
            return Visible(td) && searchFilter.Matches(td);
        }

        // Helper method for SpecialThingFilterDefs
        private bool SpecialFilterVisibleAndMatches(SpecialThingFilterDef sf, TreeNode_ThingCategory node)
        {
            return Visible(sf, node) && searchFilter.Matches(sf);
        }

        private MultiCheckboxState GetMultiCheckboxState(ThingDef tDef)
        {
            int allowedCount = 0;
            int totalCount = _cachedFuelBuildings.Count;

            foreach (var fuelBuilding in _cachedFuelBuildings)
            {
                if (fuelBuilding.AllowedFuelFilter.Allows(tDef))
                {
                    allowedCount++;
                }
            }
            if (allowedCount == 0)
            {
                return MultiCheckboxState.Off;
            }
            if (allowedCount == totalCount)
            {
                return MultiCheckboxState.On;
            }
            return MultiCheckboxState.Partial;
        }


    }
}