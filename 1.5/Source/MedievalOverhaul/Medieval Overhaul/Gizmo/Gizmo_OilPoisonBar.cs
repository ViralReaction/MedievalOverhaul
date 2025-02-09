using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Gizmo_OilChargesBar : Gizmo
    {
        private readonly CompWeaponOil comp;
        private readonly Pawn pawn; // Store the pawn

        public Gizmo_OilChargesBar(CompWeaponOil comp, Pawn pawn)
        {
            this.comp = comp;
            this.pawn = pawn; // Assign pawn
        }
        private static readonly Texture2D OilGizmoIcon = ContentFinder<Texture2D>.Get("UI/Icons/PoisonedOil", true);
        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {

            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);

            // Draw Icon
            Rect iconRect = new Rect(rect.x + 5, rect.y + 5, 64, 64);

            // **Highlight Background When Hovered**
            if (Mouse.IsOver(iconRect))
            {
                Widgets.DrawHighlight(iconRect); // Light background effect
                GUI.color = new Color(1f, 1f, 1f, 0.5f); // Slight transparency effect
                Widgets.DrawBox(iconRect); // Adds a border when hovered
            }

            GUI.DrawTexture(iconRect, OilGizmoIcon);
            GUI.color = Color.white;

            // Draw Progress Bar
            float progress = (float)comp.oilCharges / comp.maxCharges;
            Rect barRect = new Rect(iconRect.xMax + 10, rect.y + 30, rect.width - iconRect.width - 20, 20);
            Widgets.FillableBar(barRect, progress, SolidColorMaterials.NewSolidColorTexture(Color.yellow), SolidColorMaterials.NewSolidColorTexture(Color.black), false);

            // Draw Text
            Rect labelRect = new Rect(barRect.x, barRect.yMax + 5, barRect.width, 20);
            Widgets.Label(labelRect, $"{comp.oilCharges} / {comp.maxCharges} Charges");

            // Checkbox for Auto-Refuel (inside the gizmo box)
            Rect checkBoxRect = new Rect(rect.x + 10, rect.yMax - 5, rect.width - 30, 12f);
            Widgets.CheckboxLabeled(checkBoxRect, "Auto-Refuel", ref comp.autoRefuel);

            // Handle clicking
            if (Widgets.ButtonInvisible(iconRect))
            {
                TryRefillOil();
            }

            return new GizmoResult(GizmoState.Clear);
        }
        private void TryRefillOil()
        {
            if (this.pawn == null)
                return;
            Log.Message("Test");
            //if (pawn == null || comp == null)
            //    return;

            // Find oil in inventory
            //Thing oilItem = null;
            //foreach (Thing item in pawn.inventory.innerContainer)
            //{
            //    if (item.def == comp.oilRefillDef)
            //    {
            //        oilItem = item;
            //        break;
            //    }
            //}
            //if (oilItem != null)
            //{
                // Refill oil charges
               // comp.oilCharges = comp.maxCharges;
               // oilItem.SplitOff(1).Destroy();

                //Messages.Message($"Refilled {comp.oilType} for {pawn.LabelShort}'s {comp.parent.LabelShort}.", MessageTypeDefOf.PositiveEvent);
            //}
            //else
            //{
            //    Messages.Message($"No {comp.OilRefillDef.label} available in inventory to refill {oilComp.parent.LabelShort}.", MessageTypeDefOf.RejectInput);
            //}
        }
    }

}