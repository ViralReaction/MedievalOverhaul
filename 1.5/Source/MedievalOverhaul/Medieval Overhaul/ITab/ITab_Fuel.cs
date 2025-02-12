using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MedievalOverhaul
{
    public class ITab_Fuel : ITab
    {
        // UI State
        private static readonly Vector2 WinSize = new(300f, 480f);
        private readonly ThingFilterUI.UIState _fuelFilterState = new();

        // Cached Data
        private List<CompStoreFuelThing> _cachedFuelBuildings = [];
        private List<ThingDef> _cachedFuelDefs = [];
        private Dictionary<ThingCategoryDef, List<ThingDef>> _cachedCategorizedFuels = [];
        private Dictionary<ThingCategoryDef, MultiCheckboxState> _cachedCategoryStates = [];
        private Dictionary<ThingDef, MultiCheckboxState> _cachedFuelStates = [];
        private float _cachedContentHeight = 0f;
        private IList<object> _lastSelectedObjects;

        // UI Settings
        private Dictionary<ThingCategoryDef, bool> _categoryOpen = [];
        private const int LineHeight = 22;

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_Fuel()
        {
            this.size = ITab_Fuel.WinSize;
            this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
        }

        public override bool IsVisible
        {
            get
            {
                if (Hidden)
                    return false;

                var selectedObjects = Find.Selector.SelectedObjects;

                if (selectedObjects.Count == 0)
                    return false;

                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    if (selectedObjects[i] is not Thing thing ||
                        thing.Faction != Faction.OfPlayerSilentFail ||
                        thing.TryGetComp<CompStoreFuelThing>() == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        public override bool Hidden => this.IsDisabled();

        public bool IsDisabled() => Utility.LWMFuelFilterIsEnabled;

        public override void OnOpen()
        {
            base.OnOpen();
            this._fuelFilterState.quickSearch.Reset();
        }

        public override void FillTab()
        {
            var selectedObjects = Find.Selector.SelectedObjects;

            // If selection hasn't changed, skip updating.
            if (HasSelectionChanged(selectedObjects))
            {
                _cachedFuelBuildings.Clear();
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    if (selectedObjects[i] is ThingWithComps thingWithComps)
                    {
                        var comp = thingWithComps.TryGetComp<CompStoreFuelThing>();
                        if (comp != null)
                        {
                            _cachedFuelBuildings.Add(comp);
                        }
                    }
                }
                _lastSelectedObjects = new List<object>(selectedObjects); // Store new selection.

                // Update cached fuelDefs
                UpdateCachedFuelDefs();
            }

            if (!_cachedFuelBuildings.Any()) return;
            if (!_cachedFuelDefs.Any()) return;

            List<ThingCategoryDef> keys = new List<ThingCategoryDef>();
            if (_cachedCategorizedFuels.Count == 0)
            {
                HashSet<ThingCategoryDef> fuelCategories = new();
                for (int i = 0; i < _cachedFuelDefs.Count; i++)
                {
                    if (_cachedFuelDefs[i].thingCategories != null)
                        fuelCategories.UnionWith(_cachedFuelDefs[i].thingCategories);
                }

                ThingCategoryDef commonCategory = FindCommonAncestorCategory(fuelCategories);
                _cachedCategorizedFuels = GroupByHierarchy(_cachedFuelDefs, commonCategory);

                _cachedContentHeight = 0f;
                keys.Clear();
                foreach (var key in _cachedCategorizedFuels.Keys)
                {
                    keys.Add(key);
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    if (key.parent == null || !_cachedCategorizedFuels.ContainsKey(key.parent))
                    {
                        CalculateFilteredCategoryHeight(key, _cachedCategorizedFuels, _cachedFuelBuildings, null, 0, ref _cachedContentHeight);
                    }
                }
            }

            // UI Drawing
            Rect outerRect = new(0f, 0f, WinSize.x, WinSize.y);
            Rect innerRect = outerRect.ContractedBy(10f);
            innerRect.SplitHorizontally(18f, out Rect _, out Rect bottomSection);

            Widgets.DrawMenuSection(bottomSection);
            float buttonWidth = ((bottomSection.width - 2f) / 2f) - 4.5f;
            Rect clearAllButton = new Rect(bottomSection.x + 3f, bottomSection.y + 3f, buttonWidth, 24f);
            Rect allowAllButton = new Rect(clearAllButton.xMax + 3f, bottomSection.y + 3f, buttonWidth, 24f);

            if (Widgets.ButtonText(clearAllButton, "ClearAll".Translate(), true, true, true))
            {
                for (int i = 0; i < _cachedFuelBuildings.Count; i++)
                {
                    _cachedFuelBuildings[i].AllowedFuelFilter.SetDisallowAll(null, null);
                }

                ClearFuelStateCache();
                PrecomputeFuelStates(_cachedFuelDefs, _cachedFuelBuildings);
                RecalculateCategoryStates();
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }

            if (Widgets.ButtonText(allowAllButton, "AllowAll".Translate(), true, true, true, null))
            {
                for (int i = 0; i < _cachedFuelBuildings.Count; i++)
                {
                    var comp = _cachedFuelBuildings[i];
                    comp.AllowedFuelFilter.SetAllowAll(comp.parent.GetComp<CompRefuelable>().Props.fuelFilter, false);
                }
                ClearFuelStateCache();
                PrecomputeFuelStates(_cachedFuelDefs, _cachedFuelBuildings);
                RecalculateCategoryStates();
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }

            Rect searchBarRect = new Rect(bottomSection.x + 3f, clearAllButton.y + 3f + 24f, bottomSection.width - 16f - 6f, 24f);
            _fuelFilterState.quickSearch.OnGUI(searchBarRect, null, null);
            string searchFilter = _fuelFilterState.quickSearch.filter.Active ? _fuelFilterState.quickSearch.filter.Text?.ToLower() : null;

            // Scroll View
            float buttonHeight = 60f;
            float padding = 5f;
            Rect listRect = new(bottomSection.x, bottomSection.y + buttonHeight + padding, bottomSection.width - 2f, bottomSection.height - buttonHeight - padding);
            Rect viewRect = new(0f, 0f, bottomSection.width - 18f, Mathf.Max(_cachedContentHeight, listRect.y - bottomSection.y) + 10f);


            _cachedContentHeight = 0f;

            keys.Clear();
            foreach (var key in _cachedCategorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (key.parent == null || !_cachedCategorizedFuels.ContainsKey(key.parent))
                {
                    CalculateFilteredCategoryHeight(key, _cachedCategorizedFuels, _cachedFuelBuildings, null, 0, ref _cachedContentHeight);
                }
            }

            Widgets.BeginScrollView(listRect, ref _fuelFilterState.scrollPosition, viewRect);

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (key.parent == null || !_cachedCategorizedFuels.ContainsKey(key.parent))
                {
                    DrawCategoryRecursive(ref listRect, key, _cachedCategorizedFuels, 0, _cachedFuelBuildings, searchFilter);
                }
            }

            Widgets.EndScrollView();
        }

        private void PrecomputeFuelStates(List<ThingDef> fuelDefs, List<CompStoreFuelThing> fuelBuildings)
        {
            _cachedFuelStates.Clear();

            for (int i = 0; i < fuelDefs.Count; i++)
            {
                var fuelDef = fuelDefs[i];
                _cachedFuelStates[fuelDef] = FuelStateOf(fuelDef, fuelBuildings);
            }
        }

        private void DrawCategoryRecursive(ref Rect listRect, ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, int depth, List<CompStoreFuelThing> fuelBuildings, string searchFilter)
        {
            if (!_categoryOpen.ContainsKey(category))
                _categoryOpen[category] = false;

            bool hasMatchingFuels = categorizedFuels.ContainsKey(category) && categorizedFuels[category].Any(fuel => MatchesSearch(fuel, searchFilter));
            bool hasMatchingSubcategories = false;
            List<ThingCategoryDef> keys = new List<ThingCategoryDef>(categorizedFuels.Count);
            foreach (var key in categorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var sub = keys[i];
                if (sub.parent == category && MatchesSearch(sub, searchFilter))
                {
                    hasMatchingSubcategories = true;
                    break;
                }
            }

            if (searchFilter != null && !hasMatchingFuels && !hasMatchingSubcategories)
                return;

            float indent = depth * 6f;
            Rect categoryRect = new(listRect.x + indent, listRect.y - 95f, listRect.width - 44f - indent, LineHeight);
            Rect checkboxRect = new(categoryRect.xMax - 2f, categoryRect.y, 20f, 20f);

            MultiCheckboxState categoryState = CategoryStateOf(category, categorizedFuels, fuelBuildings);
            MultiCheckboxState newCategoryState = Widgets.CheckboxMulti(checkboxRect, categoryState, true);

            if (categoryState != newCategoryState && newCategoryState != MultiCheckboxState.Partial)
            {
                ClearCategoryStateCache();
                bool enable = (newCategoryState == MultiCheckboxState.On);

                SetCategoryStateRecursive(category, categorizedFuels, fuelBuildings, enable);
                RecalculateCategoryStates();
            }

            Widgets.Label(categoryRect, (_categoryOpen[category] ? "▼ " : "◢ ") + category.LabelCap);
            if (Mouse.IsOver(categoryRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                _categoryOpen[category] = !_categoryOpen[category];
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                Event.current.Use();
            }

            listRect.y += LineHeight;

            if (_categoryOpen[category])
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var subcategory = keys[i];
                    if (subcategory.parent == category)
                    {
                        DrawCategoryRecursive(ref listRect, subcategory, categorizedFuels, depth + 1, fuelBuildings, searchFilter);
                    }
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    var fuels = categorizedFuels[category];
                    for (int i = 0; i < fuels.Count; i++)
                    {
                        var fuelDef = fuels[i];
                        if (MatchesSearch(fuelDef, searchFilter))
                        {
                            DoFuelList(ref listRect, fuelDef, fuelBuildings, depth + 1);
                        }
                    }
                }
            }

        }

        private void ClearCategoryStateCache()
        {
            _cachedCategoryStates.Clear();
        }

        private void DoFuelList(ref Rect listRect, ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings, int depth)
        {
            float indent = depth * 6f;

            Rect headerRect = new(listRect.x, listRect.y - 95f, listRect.width, LineHeight);
            Rect iconRect = new(headerRect.x + indent, headerRect.y, 24f, 24f);
            Rect labelRect = new(headerRect.x + indent + 28f, headerRect.y, headerRect.width - 28f - indent, 24f);
            Rect checkboxRect = new(headerRect.xMax - 48f, headerRect.y, 20f, 20f);
            if (!_cachedFuelStates.TryGetValue(fuelDef, out MultiCheckboxState fuelState))
            {
                fuelState = FuelStateOf(fuelDef, fuelBuildings);
                _cachedFuelStates[fuelDef] = fuelState;
            }
            MultiCheckboxState newFuelState = Widgets.CheckboxMulti(checkboxRect, fuelState, true);
            if (fuelState != newFuelState && newFuelState != MultiCheckboxState.Partial)
            {
                for (int i = 0; i < fuelBuildings.Count; i++)
                {
                    fuelBuildings[i].AllowedFuelFilter.SetAllow(fuelDef, newFuelState == MultiCheckboxState.On);
                }
                _cachedFuelStates[fuelDef] = newFuelState;
                ClearCategoryStateCache();
                RecalculateCategoryStates();
            }

            Widgets.DefIcon(iconRect, fuelDef);
            Widgets.Label(labelRect, fuelDef.LabelCap);

            listRect.y += LineHeight;
        }
        private void RecalculateCategoryStates()
        {
            _cachedCategoryStates.Clear();

            List<ThingCategoryDef> keys = new List<ThingCategoryDef>(_cachedCategorizedFuels.Count);
            foreach (var key in _cachedCategorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var category = keys[i];
                _cachedCategoryStates[category] = CategoryStateOf(category, _cachedCategorizedFuels, _cachedFuelBuildings);
            }

        }

        private void ClearFuelStateCache()
        {
            _cachedFuelStates.Clear();
            _cachedCategorizedFuels.Clear();
            _cachedContentHeight = 0f;
        }

        private bool HasSelectionChanged(IList<object> selectedObjects)
        {
            if (ReferenceEquals(_lastSelectedObjects, selectedObjects))
                return false;

            if (_lastSelectedObjects == null || _lastSelectedObjects.Count != selectedObjects.Count)
                return true;

            HashSet<object> currentSelection = new(selectedObjects);
            for (int i = 0; i < _lastSelectedObjects.Count; i++)
            {
                if (!currentSelection.Contains(_lastSelectedObjects[i]))
                    return true;
            }
            return false;
        }

        private void UpdateCachedFuelDefs()
        {
            _cachedFuelDefs.Clear();

            CompRefuelable refuelable = _cachedFuelBuildings.Count > 0 ? _cachedFuelBuildings[0].parent.GetComp<CompRefuelable>() : null;
            if (refuelable?.Props.fuelFilter == null) return;

            foreach (var thingDef in refuelable.Props.fuelFilter.AllowedThingDefs)
            {
                _cachedFuelDefs.Add(thingDef);
            }

            _cachedCategorizedFuels.Clear();
            _cachedContentHeight = 0f;
        }

        private void CalculateFilteredCategoryHeight(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, List<CompStoreFuelThing> fuelBuildings, string searchFilter, int depth, ref float contentHeight)
        {
            bool hasMatchingFuels = categorizedFuels.ContainsKey(category) && categorizedFuels[category].Any(fuel => MatchesSearch(fuel, searchFilter));
            bool hasMatchingSubcategories = false;
            List<ThingCategoryDef> keys = new List<ThingCategoryDef>(categorizedFuels.Count);
            foreach (var key in categorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var sub = keys[i];
                if (sub.parent == category && MatchesSearch(sub, searchFilter))
                {
                    hasMatchingSubcategories = true;
                    break;
                }
            }

            if (searchFilter != null && !hasMatchingFuels && !hasMatchingSubcategories)
                return;

            contentHeight += LineHeight;

            if (_categoryOpen.ContainsKey(category) && _categoryOpen[category])
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var subcategory = keys[i];
                    if (subcategory.parent == category)
                    {
                        CalculateFilteredCategoryHeight(subcategory, categorizedFuels, fuelBuildings, searchFilter, depth + 1, ref contentHeight);
                    }
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    contentHeight += categorizedFuels[category].Count(f => MatchesSearch(f, searchFilter)) * LineHeight;
                }
            }
        }
        private void SetCategoryStateRecursive(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, List<CompStoreFuelThing> fuelBuildings, bool enable)
        {
            _cachedCategoryStates.Remove(category);
            bool allEnabled = enable;
            bool anyEnabled = false;
            List<ThingCategoryDef> keys = new List<ThingCategoryDef>(categorizedFuels.Count);
            foreach (var key in categorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var subcategory = keys[i];
                if (subcategory.parent == category)
                {
                    SetCategoryStateRecursive(subcategory, categorizedFuels, fuelBuildings, enable);

                    if (_cachedCategoryStates.TryGetValue(subcategory, out MultiCheckboxState subState))
                    {
                        if (subState == MultiCheckboxState.On)
                        {
                            anyEnabled = true;
                        }
                        else if (subState == MultiCheckboxState.Partial)
                        {
                            anyEnabled = true;
                            allEnabled = false;
                        }
                        else
                        {
                            allEnabled = false;
                        }
                    }
                }
            }

            if (categorizedFuels.TryGetValue(category, out var fuelList))
            {
                for (int i = 0; i < fuelList.Count; i++)
                {
                    var fuelDef = fuelList[i];

                    for (int j = 0; j < fuelBuildings.Count; j++)
                    {
                        fuelBuildings[j].AllowedFuelFilter.SetAllow(fuelDef, enable);
                    }

                    anyEnabled |= enable;
                }
            }

            MultiCheckboxState newState = allEnabled ? MultiCheckboxState.On
                                          : (anyEnabled ? MultiCheckboxState.Partial
                                          : MultiCheckboxState.Off);
            _cachedCategoryStates[category] = newState;
        }


        private bool MatchesSearch(ThingDef fuelDef, string searchFilter) => searchFilter == null || fuelDef.label.ToLower().Contains(searchFilter);

        private bool MatchesSearch(ThingCategoryDef category, string searchFilter) => searchFilter == null || category.label.ToLower().Contains(searchFilter);

        private Dictionary<ThingCategoryDef, List<ThingDef>> GroupByHierarchy(List<ThingDef> fuelDefs, ThingCategoryDef commonCategory)
        {
            Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels = new();

            for (int i = 0; i < fuelDefs.Count; i++)
            {
                var fuelDef = fuelDefs[i];
                ThingCategoryDef assignedCategory = null;
                int highestDepth = -1;

                for (int j = 0; j < fuelDef.thingCategories.Count; j++)
                {
                    var category = fuelDef.thingCategories[j];
                    if (IsDescendantOf(category, commonCategory))
                    {
                        int depth = GetCategoryDepth(category);
                        if (depth > highestDepth)
                        {
                            highestDepth = depth;
                            assignedCategory = category;
                        }
                    }
                }

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
            List<List<ThingCategoryDef>> ancestorChains = new List<List<ThingCategoryDef>>();

            foreach (var category in categories)
            {
                ancestorChains.Add(GetFullCategoryChain(category));
            }

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

            chain.Reverse();
            return chain;
        }
        private ThingCategoryDef FindDeepestCommonAncestor(List<List<ThingCategoryDef>> ancestorChains)
        {
            if (ancestorChains.Count == 0)
                return null;

            int minDepth = int.MaxValue;

            for (int i = 0; i < ancestorChains.Count; i++)
            {
                if (ancestorChains[i].Count < minDepth)
                {
                    minDepth = ancestorChains[i].Count;
                }
            }
            ThingCategoryDef commonAncestor = null;

            for (int i = 0; i < minDepth; i++)
            {
                ThingCategoryDef candidate = ancestorChains[0][i];

                bool allMatch = true;

                for (int j = 0; j < ancestorChains.Count; j++)
                {
                    if (ancestorChains[j][i] != candidate)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch)
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
            if (_cachedCategoryStates.TryGetValue(category, out MultiCheckboxState cachedState))
                return cachedState;

            bool hasEnabled = false;
            bool hasDisabled = false;
            bool hasPartial = false;

            if (categorizedFuels.TryGetValue(category, out List<ThingDef> fuelList))
            {
                for (int i = 0; i < fuelList.Count; i++)
                {
                    var fuel = fuelList[i];
                    bool allEnabled = true;
                    bool noneEnabled = true;

                    for (int j = 0; j < fuelBuildings.Count; j++)
                    {
                        var comp = fuelBuildings[j];
                        bool allows = comp.AllowedFuelFilter.Allows(fuel);
                        if (!allows)
                            allEnabled = false;
                        if (allows)
                            noneEnabled = false;
                        if (!allEnabled && !noneEnabled)
                            break;
                    }

                    if (allEnabled)
                        hasEnabled = true;
                    else if (noneEnabled)
                        hasDisabled = true;
                    else
                        hasPartial = true;
                }
            }

            List<ThingCategoryDef> keys = new List<ThingCategoryDef>(categorizedFuels.Count);
            foreach (var key in categorizedFuels.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                var subcategory = keys[i];
                if (subcategory.parent == category)
                {
                    MultiCheckboxState subState = CategoryStateOf(subcategory, categorizedFuels, fuelBuildings);

                    if (subState == MultiCheckboxState.Partial)
                        hasPartial = true;
                    if (subState == MultiCheckboxState.On)
                        hasEnabled = true;
                    if (subState == MultiCheckboxState.Off)
                        hasDisabled = true;
                }
            }

            MultiCheckboxState finalState = hasPartial || (hasEnabled && hasDisabled) ? MultiCheckboxState.Partial
                                         : hasEnabled ? MultiCheckboxState.On
                                         : MultiCheckboxState.Off;

            _cachedCategoryStates[category] = finalState;
            return finalState;
        }

        private void CalculateCategoryHeight(ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, int depth, ref float contentHeight)
        {
            contentHeight += LineHeight;

            if (_categoryOpen.ContainsKey(category) && _categoryOpen[category])
            {
                List<ThingCategoryDef> keys = new List<ThingCategoryDef>(categorizedFuels.Count);
                foreach (var key in categorizedFuels.Keys)
                {
                    keys.Add(key);
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    var subcategory = keys[i];
                    if (subcategory.parent == category)
                    {
                        CalculateCategoryHeight(subcategory, categorizedFuels, depth + 1, ref contentHeight);
                    }
                }
                if (categorizedFuels.ContainsKey(category))
                {
                    contentHeight += categorizedFuels[category].Count * LineHeight;
                }
            }
        }

    }
}