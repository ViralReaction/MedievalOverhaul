using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace MedievalOverhaul
{
    public class ITab_FuelCustom : ITab
    {
        private static readonly Vector2 WinSize = new (300f, 480f);
        private readonly ThingFilterUI_Fuel.UIState fuelFilterState = new ();
        private List<CompRefuelableCustom> _cachedFuelBuildings = [];
        private IList<object> _lastSelectedObjects;

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_FuelCustom()
        {
            this.size = ITab_FuelCustom.WinSize;
            this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.fuelFilterState.quickSearch.Reset();
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
                        var comp = thingWithComps.TryGetComp<CompRefuelableCustom>();
                        if (comp != null)
                        {
                            _cachedFuelBuildings.Add(comp);
                        }
                    }
                }
                _lastSelectedObjects = new List<object>(selectedObjects); // Store new selection.
            }
            if (!_cachedFuelBuildings.Any()) return;
            new Rect(0.0f, 0.0f, ITab_FuelCustom.WinSize.x, ITab_FuelCustom.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out Rect bottom);
            ThingFilterUI_FuelCustom.DoThingFilterConfigWindow(bottom, this.fuelFilterState, _cachedFuelBuildings[0].AllowedFuelFilter, _cachedFuelBuildings, _cachedFuelBuildings[0].Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
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
