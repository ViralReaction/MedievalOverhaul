using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MedievalOverhaul
{
    public class ITab_Fuel : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 480f);
        private ThingFilterUI.UIState fuelFilterState = new();
        private Dictionary<ThingCategoryDef, bool> categoryOpen = new();
        private const int LineHeight = 22;

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_Fuel()
        {
            this.size = ITab_Fuel.WinSize;
            this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
        }

        public override bool IsVisible => !this.Hidden && Find.Selector.SelectedObjects.All(x => x is Thing thing && thing.Faction == Faction.OfPlayerSilentFail && thing.TryGetComp<CompStoreFuelThing>() != null);

        public override bool Hidden => this.IsDisabled();

        public bool IsDisabled() => Utility.LWMFuelFilterIsEnabled;

        public override void OnOpen()
        {
            base.OnOpen();
            this.fuelFilterState.quickSearch.Reset();
        }

        public override void FillTab()
        {
            var fuelBuildings = Find.Selector.SelectedObjects
                .Select(o => (o as ThingWithComps)?.TryGetComp<CompStoreFuelThing>())
                .Where(comp => comp != null)
                .ToList();

            if (!fuelBuildings.Any()) return;

            var refuelable = fuelBuildings.First().parent.GetComp<CompRefuelable>();
            if (refuelable?.Props.fuelFilter == null) return;

            var fuelDefs = refuelable.Props.fuelFilter.AllowedThingDefs.ToList();
            HashSet<ThingCategoryDef> fuelCategories = new();
            foreach (var fuel in fuelDefs)
            {
                if (fuel.thingCategories != null)
                    fuelCategories.UnionWith(fuel.thingCategories);
            }

            ThingCategoryDef commonCategory = FindCommonAncestorCategory(fuelCategories);
            Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels = GroupByHierarchy(fuelDefs, commonCategory);

            Rect outerRect = new(0f, 0f, WinSize.x, WinSize.y);
            Rect innerRect = outerRect.ContractedBy(10f);
            innerRect.SplitHorizontally(18f, out Rect _, out Rect bottomSection);

            Widgets.DrawMenuSection(bottomSection);
            float buttonWidth = ((bottomSection.width - 2f) / 2f) - 4.5f;
            Rect clearAllButton = new Rect(bottomSection.x + 3f, bottomSection.y + 3f, buttonWidth, 24f);
            Rect allowAllButton = new Rect(clearAllButton.xMax + 3f, clearAllButton.y, buttonWidth, 24f);

            if (Widgets.ButtonText(clearAllButton, "ClearAll".Translate(), true, true, true))
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetDisallowAll(null, null);
                }
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }

            if (Widgets.ButtonText(allowAllButton, "AllowAll".Translate(), true, true, true, null))
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetAllowAll(refuelable.Props.fuelFilter, false);
                }
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }

            Rect searchBarRect = new Rect(bottomSection.x + 3f, clearAllButton.y + 3f + 24f, bottomSection.width - 16f - 6f, 24f);
            fuelFilterState.quickSearch.OnGUI(searchBarRect, null, null);
            string searchFilter = fuelFilterState.quickSearch.filter.Active ? fuelFilterState.quickSearch.filter.Text?.ToLower() : null;

            bottomSection.yMin = allowAllButton.yMax - 24f;
            bottomSection.xMax -= 4f;
            bottomSection.yMax -= 6f;

            // Adjust listRect to account for scrollbar width

            float contentHeight = 0f;

            foreach (var rootCategory in categorizedFuels.Keys.Where(c => c.parent == null || !categorizedFuels.ContainsKey(c.parent)))
            {
                CalculateFilteredCategoryHeight(rootCategory, categorizedFuels, fuelBuildings, searchFilter, 0, ref contentHeight);
            }

            // Offset height to move the scroll area below the buttons
            float buttonHeight = 60f; // Height of the buttons
            float padding = 5f;       // Extra spacing
            Rect listRect = new(bottomSection.x, bottomSection.y + buttonHeight + padding, bottomSection.width - 2f, bottomSection.height - buttonHeight - padding);
            Rect viewRect = new(0f, 0f, bottomSection.width - 18f, contentHeight + 10f);
            

            Widgets.BeginScrollView(listRect, ref fuelFilterState.scrollPosition, viewRect);

            foreach (var rootCategory in categorizedFuels.Keys.Where(c => c.parent == null || !categorizedFuels.ContainsKey(c.parent)))
            {
                DrawCategoryRecursive(ref listRect, rootCategory, categorizedFuels, 0, fuelBuildings, searchFilter);
            }

            Widgets.EndScrollView();
        }

        private void CalculateFilteredCategoryHeight(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, List<CompStoreFuelThing> fuelBuildings, string searchFilter, int depth, ref float contentHeight)
        {
            bool hasMatchingFuels = categorizedFuels.ContainsKey(category) && categorizedFuels[category].Any(fuel => MatchesSearch(fuel, searchFilter));
            bool hasMatchingSubcategories = categorizedFuels.Keys.Any(sub => sub.parent == category && MatchesSearch(sub, searchFilter));

            if (searchFilter != null && !hasMatchingFuels && !hasMatchingSubcategories)
                return; // Skip categories that don't match search

            contentHeight += LineHeight; // Account for category height

            if (categoryOpen.ContainsKey(category) && categoryOpen[category])
            {
                foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
                {
                    CalculateFilteredCategoryHeight(subcategory, categorizedFuels, fuelBuildings, searchFilter, depth + 1, ref contentHeight);
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    contentHeight += categorizedFuels[category].Count(f => MatchesSearch(f, searchFilter)) * LineHeight;
                }
            }
        }

        private void DrawCategoryRecursive(ref Rect listRect, ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, int depth, List<CompStoreFuelThing> fuelBuildings, string searchFilter)
        {
            if (!categoryOpen.ContainsKey(category))
                categoryOpen[category] = false;

            // Check if this category or any of its fuels match the search
            bool hasMatchingFuels = categorizedFuels.ContainsKey(category) && categorizedFuels[category].Any(fuel => MatchesSearch(fuel, searchFilter));
            bool hasMatchingSubcategories = categorizedFuels.Keys.Any(sub => sub.parent == category && MatchesSearch(sub, searchFilter));

            // Skip categories that don't match the search
            if (searchFilter != null && !hasMatchingFuels && !hasMatchingSubcategories)
                return;

            float indent = depth * 12f;
            Rect categoryRect = new(listRect.x + indent, listRect.y - 95f, listRect.width - 44f - indent, LineHeight);
            Rect checkboxRect = new(categoryRect.xMax - 2f, categoryRect.y, 20f, 20f);

            MultiCheckboxState categoryState = CategoryStateOf(category, categorizedFuels, fuelBuildings);
            MultiCheckboxState newCategoryState = Widgets.CheckboxMulti(checkboxRect, categoryState, true);

            // **Apply state change to all children when toggling**
            if (categoryState != newCategoryState && newCategoryState != MultiCheckboxState.Partial)
            {
                SetCategoryStateRecursive(category, categorizedFuels, fuelBuildings, newCategoryState == MultiCheckboxState.On);
            }

            Widgets.Label(categoryRect, (categoryOpen[category] ? "▼ " : "◢ ") + category.LabelCap);
            if (Mouse.IsOver(categoryRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                categoryOpen[category] = !categoryOpen[category];
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                Event.current.Use();
            }

            listRect.y += LineHeight;

            if (categoryOpen[category])
            {
                foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
                {
                    DrawCategoryRecursive(ref listRect, subcategory, categorizedFuels, depth + 1, fuelBuildings, searchFilter);
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    foreach (ThingDef fuelDef in categorizedFuels[category].Where(f => MatchesSearch(f, searchFilter)))
                    {
                        DoFuelList(ref listRect, fuelDef, fuelBuildings, depth + 1);
                    }
                }
            }
        }

        private void SetCategoryStateRecursive(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, List<CompStoreFuelThing> fuelBuildings, bool allow)
        {
            // Enable/Disable all fuels in this category
            if (categorizedFuels.ContainsKey(category))
            {
                foreach (ThingDef fuelDef in categorizedFuels[category])
                {
                    foreach (CompStoreFuelThing comp in fuelBuildings)
                    {
                        comp.AllowedFuelFilter.SetAllow(fuelDef, allow);
                    }
                }
            }

            // Recursively apply state to subcategories
            foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
            {
                SetCategoryStateRecursive(subcategory, categorizedFuels, fuelBuildings, allow);
            }

            // **Fully Recalculate Parent States**
            ThingCategoryDef parent = category.parent;
            while (parent != null)
            {
                MultiCheckboxState newState = CategoryStateOf(parent, categorizedFuels, fuelBuildings);

                // Ensure parent updates correctly based on all child categories
                if (newState == MultiCheckboxState.Partial)
                    break; // Stop updating if already Partial (no need to go higher)

                parent = parent.parent;
            }
        }

        private void DoFuelList(ref Rect listRect, ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings, int depth)
        {
            float indent = depth * 6f; // Proper indentation for nested fuels

            Rect headerRect = new(listRect.x, listRect.y - 95f, listRect.width, LineHeight);
            Rect iconRect = new(headerRect.x + indent, headerRect.y, 24f, 24f);
            Rect labelRect = new(headerRect.x + indent + 28f, headerRect.y, headerRect.width - 28f - indent, 24f);
            Rect checkboxRect = new(headerRect.xMax - 48f, headerRect.y, 20f, 20f);

            Widgets.DefIcon(iconRect, fuelDef);

            Text.Font = GameFont.Small;
            Widgets.Label(labelRect, fuelDef.LabelCap);
            Text.Font = GameFont.Small; // Reset font size after drawing

            MultiCheckboxState fuelState = FuelStateOf(fuelDef, fuelBuildings);
            MultiCheckboxState newFuelState = Widgets.CheckboxMulti(checkboxRect, fuelState, true);

            if (fuelState != newFuelState && newFuelState != MultiCheckboxState.Partial)
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetAllow(fuelDef, newFuelState == MultiCheckboxState.On);
                }
            }

            listRect.y += LineHeight; // Move to the next line to avoid overlap
        }

        private bool MatchesSearch(ThingDef fuelDef, string searchFilter) => searchFilter == null || fuelDef.label.ToLower().Contains(searchFilter);

        private bool MatchesSearch(ThingCategoryDef category, string searchFilter) => searchFilter == null || category.label.ToLower().Contains(searchFilter);

        private Dictionary<ThingCategoryDef, List<ThingDef>> GroupByHierarchy(List<ThingDef> fuelDefs, ThingCategoryDef commonCategory)
        {
            Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels = new();

            foreach (var fuelDef in fuelDefs)
            {
                ThingCategoryDef assignedCategory = fuelDef.thingCategories
                    .Where(category => IsDescendantOf(category, commonCategory))
                    .OrderByDescending(category => GetCategoryDepth(category))
                    .FirstOrDefault();

                if (assignedCategory == null)
                    assignedCategory = commonCategory;

                if (!categorizedFuels.ContainsKey(assignedCategory))
                    categorizedFuels[assignedCategory] = new List<ThingDef>();

                categorizedFuels[assignedCategory].Add(fuelDef);
            }

            return categorizedFuels;
        }

        private int GetCategoryDepth(ThingCategoryDef category)
        {
            int depth = 0;
            while (category != null)
            {
                category = category.parent;
                depth++;
            }
            return depth;
        }

        private bool IsDescendantOf(ThingCategoryDef category, ThingCategoryDef ancestor)
        {
            while (category != null)
            {
                if (category == ancestor) return true;
                category = category.parent;
            }
            return false;
        }
        private ThingCategoryDef FindCommonAncestorCategory(HashSet<ThingCategoryDef> categories)
        {
            if (categories == null || categories.Count == 0)
            {
                return null;
            }
            // Step 1: Get the full ancestor chains for each category
            List<List<ThingCategoryDef>> ancestorChains = categories
                .Select(GetFullCategoryChain)
                .ToList();

            // Step 2: Find the deepest common ancestor
            ThingCategoryDef commonAncestor = FindDeepestCommonAncestor(ancestorChains);
            return commonAncestor;
        }
        private List<ThingCategoryDef> GetFullCategoryChain(ThingCategoryDef category)
        {
            List<ThingCategoryDef> chain = new();

            while (category != null)
            {
                chain.Add(category);
                category = category.parent;
            }

            chain.Reverse(); // Root first, child last
            return chain;
        }
        private ThingCategoryDef FindDeepestCommonAncestor(List<List<ThingCategoryDef>> ancestorChains)
        {
            if (ancestorChains.Count == 0)
                return null;

            int minDepth = ancestorChains.Min(chain => chain.Count);

            ThingCategoryDef commonAncestor = null;

            for (int i = 0; i < minDepth; i++)
            {
                ThingCategoryDef candidate = ancestorChains[0][i];

                if (ancestorChains.All(chain => chain[i] == candidate))
                {
                    commonAncestor = candidate;
                }
                else
                {
                    break;
                }
            }

            return commonAncestor;
        }

        private MultiCheckboxState FuelStateOf(ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings)
        {
            int enabledCount = fuelBuildings.Count(comp => comp.AllowedFuelFilter.Allows(fuelDef));

            if (enabledCount == 0)
                return MultiCheckboxState.Off;
            if (enabledCount == fuelBuildings.Count)
                return MultiCheckboxState.On;

            return MultiCheckboxState.Partial;
        }

        private MultiCheckboxState CategoryStateOf(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, List<CompStoreFuelThing> fuelBuildings)
        {
            bool hasEnabled = false;
            bool hasDisabled = false;
            bool hasPartial = false;

            // Check direct fuels in this category
            if (categorizedFuels.ContainsKey(category))
            {
                foreach (ThingDef fuel in categorizedFuels[category])
                {
                    bool allEnabled = fuelBuildings.All(comp => comp.AllowedFuelFilter.Allows(fuel));
                    bool noneEnabled = fuelBuildings.All(comp => !comp.AllowedFuelFilter.Allows(fuel));

                    if (allEnabled)
                        hasEnabled = true;
                    else if (noneEnabled)
                        hasDisabled = true;
                    else
                        hasPartial = true; // Some fuels are mixed, so category should be Partial
                }
            }

            // Recursively check subcategories
            foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
            {
                MultiCheckboxState subState = CategoryStateOf(subcategory, categorizedFuels, fuelBuildings);

                if (subState == MultiCheckboxState.Partial)
                    hasPartial = true; // If any child is Partial, parent should be Partial
                if (subState == MultiCheckboxState.On)
                    hasEnabled = true;
                if (subState == MultiCheckboxState.Off)
                    hasDisabled = true;
            }

            // Determine final state
            if (hasPartial || (hasEnabled && hasDisabled))
                return MultiCheckboxState.Partial; // If any mix exists, return Partial
            if (hasEnabled)
                return MultiCheckboxState.On;
            return MultiCheckboxState.Off;
        }

        private void CalculateCategoryHeight(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, int depth, ref float contentHeight)
        {
            contentHeight += LineHeight; // Always account for the category itself

            if (categoryOpen.ContainsKey(category) && categoryOpen[category])
            {
                foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
                {
                    CalculateCategoryHeight(subcategory, categorizedFuels, depth + 1, ref contentHeight);
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    contentHeight += categorizedFuels[category].Count * LineHeight;
                }
            }
        }

    }
}