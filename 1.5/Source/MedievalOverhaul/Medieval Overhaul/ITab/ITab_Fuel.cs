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
        private ThingFilterUI.UIState fuelFilterState = new ();
        private Dictionary<ThingCategoryDef, bool> categoryOpen = new(); // Tracks open/closed categories
        private const int LineHeight = 22;


        private const int lineHeight = 22;

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_Fuel()
        {
            this.size = ITab_Fuel.WinSize;
            this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
        }

        public override bool IsVisible => !this.Hidden && Find.Selector.SelectedObjects.All(x => x is Thing thing && thing.Faction == Faction.OfPlayerSilentFail && thing.TryGetComp<CompStoreFuelThing>() != null);

        public override bool Hidden => this.IsDisabled();

        public bool IsDisabled() => Utility.LWMFuelFilterIsEnabled ;

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
            var categorizedFuels = fuelDefs
                .Where(fuelDef => fuelDef.thingCategories != null && fuelDef.thingCategories.Any())
                .GroupBy(fuelDef => fuelDef.thingCategories.First())
                .ToDictionary(g => g.Key, g => g.ToList());

            Rect outerRect = new(0f, 0f, WinSize.x, WinSize.y);
            Rect innerRect = outerRect.ContractedBy(10f);
            innerRect.SplitHorizontally(18f, out _, out Rect bottomSection);

            // Draw Section
            Widgets.DrawMenuSection(bottomSection);
            //float num = bottomSection.width - 2f;
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

            // Adjust layout after buttons
            bottomSection.yMin = allowAllButton.yMax - 24f;
            bottomSection.xMax -= 4f;
            bottomSection.yMax -= 6f;
            Rect listRect = new(bottomSection.x, bottomSection.y, bottomSection.width - 16f, bottomSection.height - 30f);
            //Rect listRect = new(bottomSection.x, bottomSection.y, bottomSection.width - 16f, bottomSection.height - 30f);
            Rect viewRect = new(0f, 0f, listRect.width, fuelDefs.Count * LineHeight + 80);

            Widgets.BeginScrollView(listRect, ref fuelFilterState.scrollPosition, viewRect);
            
            foreach (var category in categorizedFuels.Keys)
            {
                if (!categoryOpen.ContainsKey(category))
                    categoryOpen[category] = false;

                Rect categoryRect = new(listRect.x, listRect.y, listRect.width - 44f, LineHeight);
                Rect checkboxRect = new(categoryRect.xMax + 2f, categoryRect.y, 20f, 20f);

                MultiCheckboxState categoryState = CategoryStateOf(category, categorizedFuels[category], fuelBuildings);
                MultiCheckboxState newCategoryState = Widgets.CheckboxMulti(checkboxRect, categoryState, true);
                
                if (categoryState != newCategoryState && newCategoryState != MultiCheckboxState.Partial)
                {
                    foreach (ThingDef fuelDef in categorizedFuels[category])
                    {
                        foreach (CompStoreFuelThing comp in fuelBuildings)
                        {
                            comp.AllowedFuelFilter.SetAllow(fuelDef, newCategoryState == MultiCheckboxState.On);
                        }
                    }
                }

                Widgets.Label(categoryRect, (categoryOpen[category] ? "▼ " : "▶ ") + category.LabelCap);
                if (Mouse.IsOver(categoryRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    categoryOpen[category] = !categoryOpen[category];
                    SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    Event.current.Use();
                }

                listRect.y += LineHeight;
                
                if (categoryOpen[category])
                {
                    foreach (ThingDef fuelDef in categorizedFuels[category])
                    {
                        DoFuelList(ref listRect, fuelDef, fuelBuildings);
                    }
                }
            }

            Widgets.EndScrollView();
        }

        private void DoFuelList(ref Rect listRect, ThingDef fuelDef, List<CompStoreFuelThing> fuelBuildings)
        {
            Rect headerRect = listRect.TopPartPixels(24);
            Rect checkboxRect = new(headerRect.xMax - 42f, headerRect.y, 20, 20);
            Rect iconRect = new(headerRect.x, headerRect.y, 24f, 24f);
            Rect labelRect = new(headerRect.x + 28f, headerRect.y, headerRect.width - 28f, 24f);

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

        private MultiCheckboxState CategoryStateOf(ThingCategoryDef category, List<ThingDef> fuels, List<CompStoreFuelThing> fuelBuildings)
        {
            int enabledCount = fuels.Count(fuel => fuelBuildings.All(comp => comp.AllowedFuelFilter.Allows(fuel)));

            if (enabledCount == 0)
                return MultiCheckboxState.Off;
            if (enabledCount == fuels.Count)
                return MultiCheckboxState.On;
            return MultiCheckboxState.Partial;
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

    }
}