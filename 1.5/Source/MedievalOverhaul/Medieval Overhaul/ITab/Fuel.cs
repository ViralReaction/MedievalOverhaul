using RimWorld;
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
            innerRect.SplitHorizontally(18f, out _, out Rect bottomSection);

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

            bottomSection.yMin = allowAllButton.yMax - 24f;
            bottomSection.xMax -= 4f;
            bottomSection.yMax -= 6f;

            // Adjust listRect to account for scrollbar width

            float contentHeight = 0f;
            foreach (var rootCategory in categorizedFuels.Keys.Where(c => c.parent == null || !categorizedFuels.ContainsKey(c.parent)))
            {
                CalculateCategoryHeight(rootCategory, categorizedFuels, 0, ref contentHeight);
            }


            // Offset height to move the scroll area below the buttons
            float buttonHeight = 30f; // Height of the buttons
            float padding = 5f;       // Extra spacing
            Rect listRect = new(bottomSection.x, bottomSection.y + buttonHeight + padding, bottomSection.width - 2f, bottomSection.height - buttonHeight - padding);
            Rect viewRect = new(0f, 0f, bottomSection.width - 18f, contentHeight + 10f);

            Widgets.BeginScrollView(listRect, ref fuelFilterState.scrollPosition, viewRect);

            foreach (var rootCategory in categorizedFuels.Keys.Where(c => c.parent == null || !categorizedFuels.ContainsKey(c.parent)))
            {
                DrawCategoryRecursive(ref listRect, rootCategory, categorizedFuels, 0, fuelBuildings);
            }

            Widgets.EndScrollView();
        }


        private void DrawCategoryRecursive(ref Rect listRect, ThingCategoryDef category, Dictionary<ThingCategoryDef, List<ThingDef>> categorizedFuels, int depth, List<CompStoreFuelThing> fuelBuildings)
        {
            if (!categoryOpen.ContainsKey(category))
                categoryOpen[category] = false;

            float indent = depth * 6f;
            Rect categoryRect = new(listRect.x + indent - 4f, listRect.y - 44f, listRect.width - 44f - indent, LineHeight);
            Rect checkboxRect = new(categoryRect.xMax - 4f, categoryRect.y, 20f, 20f);

            MultiCheckboxState categoryState = categorizedFuels.ContainsKey(category)
                ? CategoryStateOf(category, categorizedFuels[category], fuelBuildings)
                : MultiCheckboxState.Off;

            MultiCheckboxState newCategoryState = Widgets.CheckboxMulti(checkboxRect, categoryState, true);

            if (categoryState != newCategoryState && newCategoryState != MultiCheckboxState.Partial)
            {
                if (categorizedFuels.ContainsKey(category))
                {
                    foreach (ThingDef fuelDef in categorizedFuels[category])
                    {
                        foreach (CompStoreFuelThing comp in fuelBuildings)
                        {
                            comp.AllowedFuelFilter.SetAllow(fuelDef, newCategoryState == MultiCheckboxState.On);
                        }
                    }
                }
            }

            Widgets.Label(categoryRect, (categoryOpen[category] ? "▼ " : "◢ ") + category.LabelCap);
            if (Mouse.IsOver(categoryRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                categoryOpen[category] = !categoryOpen[category];
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                Event.current.Use();
            }

            // **Ensure listRect is updated immediately after drawing the category**
            listRect.y += LineHeight;

            if (categoryOpen[category])
            {
                foreach (var subcategory in categorizedFuels.Keys.Where(c => c.parent == category))
                {
                    DrawCategoryRecursive(ref listRect, subcategory, categorizedFuels, depth + 1, fuelBuildings);
                }

                if (categorizedFuels.ContainsKey(category))
                {
                    foreach (ThingDef fuelDef in categorizedFuels[category])
                    {
                        DoFuelList(ref listRect, fuelDef, fuelBuildings, depth + 1);
                    }
                }
            }
        }


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
                Log.Message("[ITab_Fuel] No categories detected.");
                return null;
            }

            Log.Message($"[ITab_Fuel] Detected {categories.Count} categories: {string.Join(", ", categories.Select(c => c.defName))}");

            // Step 1: Get the full ancestor chains for each category
            List<List<ThingCategoryDef>> ancestorChains = categories
                .Select(GetFullCategoryChain)
                .ToList();

            // Step 2: Find the deepest common ancestor
            ThingCategoryDef commonAncestor = FindDeepestCommonAncestor(ancestorChains);

            Log.Message($"[ITab_Fuel] Common Ancestor Found: {commonAncestor?.defName ?? "None"}");

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
        private void DoFuelList(ref Rect listRect, ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings, int depth)
        {
            float indent = depth * 6f;
            Rect headerRect = listRect.TopPartPixels(24);
            headerRect.y -= 44f;
            Rect checkboxRect = new(headerRect.xMax - 48f, headerRect.y, 20, 20);
            Rect iconRect = new(headerRect.x - 4f + indent, headerRect.y, 24f, 24f);
            Rect labelRect = new(headerRect.x + 28f - 4f + indent, headerRect.y, headerRect.width - 28f, 24f);

            Widgets.DefIcon(iconRect, fuelDef);
            Widgets.Label(labelRect, fuelDef.LabelCap);

            MultiCheckboxState fuelState = FuelStateOf(fuelDef, fuelBuildings);
            MultiCheckboxState newFuelState = Widgets.CheckboxMulti(checkboxRect, fuelState, true);

            if (fuelState != newFuelState && newFuelState != MultiCheckboxState.Partial)
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetAllow(fuelDef, newFuelState == MultiCheckboxState.On);
                }
            }

            listRect.y += LineHeight;
        }

        private MultiCheckboxState FuelStateOf(ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings)
        {
            int count = fuelBuildings.Count(x => x.AllowedFuelFilter.Allows(fuelDef));

            if (count > 0)
            {
                if (count == fuelBuildings.Count)
                {
                    return MultiCheckboxState.On; // All selected buildings allow this fuel
                }
                return MultiCheckboxState.Partial; // Some allow, some don't
            }
            return MultiCheckboxState.Off; // None allow it
        }

        private MultiCheckboxState CategoryStateOf(ThingCategoryDef category, List<ThingDef> fuels, List<CompStoreFuelThing> fuelBuildings)
        {
            int enabledCount = fuels.Count(fuel => fuelBuildings.All(comp => comp.AllowedFuelFilter.Allows(fuel)));

            if (enabledCount == 0)
                return MultiCheckboxState.Off;
            if (enabledCount == fuels.Count)
                return MultiCheckboxState.On;
            return MultiCheckboxState.Partial;
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