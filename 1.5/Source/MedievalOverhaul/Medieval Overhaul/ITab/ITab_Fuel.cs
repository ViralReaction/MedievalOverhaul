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
            List<CompStoreFuelThing> fuelBuildings = Find.Selector.SelectedObjects
                .Select(o => (o as ThingWithComps)?.TryGetComp<CompStoreFuelThing>())
                .Where(comp => comp != null)
                .ToList();

            if (!fuelBuildings.Any())
            {
                return;
            }

            CompRefuelable refuelable = fuelBuildings.First().parent.GetComp<CompRefuelable>();
            if (refuelable == null || refuelable.Props.fuelFilter == null)
            {
                return; // No fuel filter, exit
            }

            List<ThingDef> fuelDefs = refuelable.Props.fuelFilter.AllowedThingDefs.ToList();
            var categorizedFuels = fuelDefs
                .Where(fuelDef => fuelDef.thingCategories != null && fuelDef.thingCategories.Any())
                .GroupBy(fuelDef => fuelDef.thingCategories.First()) // Use first category
                .ToDictionary(g => g.Key, g => g.ToList());

            Rect outRect = new Rect(0f, 0f, ITab_Fuel.WinSize.x, ITab_Fuel.WinSize.y).ContractedBy(10f);
            Rect buttonRect = new Rect(outRect.x, outRect.y, (outRect.width - 2f) / 2f, 24f);
            if (Widgets.ButtonText(buttonRect, "ClearAll".Translate(), true, true, true))
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetDisallowAll(null, null);
                }
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            Rect allowAllRect = new Rect(buttonRect.xMax + 3f, buttonRect.y, buttonRect.width, 24f);
            if (Widgets.ButtonText(allowAllRect, "AllowAll".Translate(), true, true, true))
            {
                foreach (var comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetAllowAll(refuelable.Props.fuelFilter, false);
                }
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }
            outRect.yMin += buttonRect.height + 6;
            Rect listRect = new Rect(0f, 2f, 280, 9999f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - GUI.skin.verticalScrollbar.fixedWidth - 1f, fuelDefs.Count * lineHeight + 80);
            Widgets.BeginScrollView(outRect, ref fuelFilterState.scrollPosition, viewRect);

            foreach (var category in categorizedFuels.Keys)
            {
                if (!categoryOpen.ContainsKey(category))
                {
                    categoryOpen[category] = false; // Default to collapsed
                }

                // Draw category label & checkbox
                Rect categoryRect = new Rect(listRect.x, listRect.y, listRect.width - 24f, lineHeight);
                Rect checkboxRect = new Rect(categoryRect.xMax, categoryRect.y, 20f, 20f);

                // ✅ Check if all/some/none of the fuels in this category are enabled
                MultiCheckboxState categoryState = CategoryStateOf(category, categorizedFuels[category], fuelBuildings);
                MultiCheckboxState newCategoryState = Widgets.CheckboxMulti(checkboxRect, categoryState, true);

                // ✅ Click on checkbox: Enable/Disable all fuels in category
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

                // ✅ Click on label: Expand/Collapse category
                Widgets.Label(categoryRect, (categoryOpen[category] ? "▼ " : "▶ ") + category.LabelCap);
                if (Mouse.IsOver(categoryRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    categoryOpen[category] = !categoryOpen[category];
                    SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    Event.current.Use();
                }

                listRect.y += lineHeight;

                // Show fuels if category is expanded
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
            Rect checkboxRect = new Rect(headerRect.x + headerRect.width - 48f, headerRect.y, 20, 20);

            Rect iconRect = new Rect(headerRect.x, headerRect.y, 24f, 24f);
            Widgets.DefIcon(iconRect, fuelDef);

            // Adjust label position to the right of the icon
            Rect labelRect = new Rect(headerRect.x + 28f, headerRect.y, headerRect.width - 28f, 24f);
            Widgets.Label(labelRect, fuelDef.LabelCap);


            MultiCheckboxState fuelState = FuelStateOf(fuelDef, fuelBuildings);
            MultiCheckboxState newFuelState = Widgets.CheckboxMulti(checkboxRect, fuelState, true);

            if (fuelState != newFuelState && newFuelState != MultiCheckboxState.Partial)
            {
                foreach (CompStoreFuelThing comp in fuelBuildings)
                {
                    comp.AllowedFuelFilter.SetAllow(fuelDef, newFuelState == MultiCheckboxState.On);
                }
            }

            listRect.y += lineHeight;
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