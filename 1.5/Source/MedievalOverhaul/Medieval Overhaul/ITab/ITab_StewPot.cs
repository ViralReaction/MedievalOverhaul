using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class ITab_StewPot : ITab
    {
        private static readonly Vector2 WinSize = new (300f, 480f);
        private readonly ThingFilterUI_Fuel.UIState fuelFilterState = new ();
        private List<ICompFuelHandler> _cachedFuelBuildings = new();
        private IList<object> _lastSelectedObjects;

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_StewPot()
        {
            this.size = ITab_StewPot.WinSize;
            this.labelKey = "DankPyon_StewPot_Itab";
        }

        public override bool IsVisible => !this.Hidden;

        public override bool Hidden => this.IsDisabled();

       public bool IsDisabled() => false;

        public override void OnOpen()
        {
            base.OnOpen();
            this.fuelFilterState.quickSearch.Reset();
        }

        //public override void FillTab()
        //{
        //    CompRefuelableStat comp1 = this.SelBuilding.GetComp<CompRefuelableStat>();
        //    new Rect(0.0f, 0.0f, ITab_StewPot.WinSize.x, ITab_StewPot.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out Rect bottom);
        //    ThingFilterUI.DoThingFilterConfigWindow(bottom, this.fuelFilterState, comp1.AllowedFuelFilter, comp1.Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
        //}
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
                        foreach (var comp in thingWithComps.AllComps)
                        {
                            if (comp is ICompFuelHandler fuelHandler)
                            {
                                _cachedFuelBuildings.Add(fuelHandler);
                            }
                        }
                    }
                }
                _lastSelectedObjects = new List<object>(selectedObjects);
            }
            if (!_cachedFuelBuildings.Any()) return;

            var firstComp = _cachedFuelBuildings[0];
            var firstBuilding = (firstComp as ThingComp)?.parent;
            if (firstBuilding == null)
                return;
            new Rect(0.0f, 0.0f, ITab_StewPot.WinSize.x, ITab_StewPot.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out Rect bottom);
            var refuelComp = firstBuilding.TryGetComp<CompRefuelableStat>();
            ThingFilterUI_Fuel.DoThingFilterConfigWindow(bottom, this.fuelFilterState, firstComp.AllowedFuelFilter, _cachedFuelBuildings, refuelComp.Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
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
    }
}