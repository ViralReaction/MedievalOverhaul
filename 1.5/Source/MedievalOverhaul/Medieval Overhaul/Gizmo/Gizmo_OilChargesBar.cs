using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Gizmo_OilChargesBar : Gizmo
    {
        private readonly CompWeaponOil comp;
        private readonly Pawn pawn; // Store the pawn

        public Gizmo_OilChargesBar(CompWeaponOil comp, Pawn pawn)
        {
            this.comp = comp;
            this.pawn = pawn; // Assign pawn
        }
        public static readonly Texture2D OilGizmoIcon = ContentFinder<Texture2D>.Get("UI/Icons/WeaponOil", true);
        public override float GetWidth(float maxWidth) => 140f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Material material = (this.disabled || parms.lowLight) ? TexUI.GrayscaleGUI : null;
            GenUI.DrawTextureWithMaterial(rect, parms.shrunk ? BGTextureShrunk : BGTexture, material, default(Rect));

            // Draw Icon
            Rect iconRect = new Rect(rect.x + 5, rect.y + 5, 64, 64);

            // **Highlight Background When Hovered**
            if (Mouse.IsOver(iconRect))
            {
                Widgets.DrawHighlight(iconRect); // Light background effect
                GUI.color = new Color(1f, 1f, 1f, 0.5f); // Slight transparency effect
                Widgets.DrawBox(iconRect); // Adds a border when hovered
                TooltipHandler.TipRegion(iconRect, () => "DankPyon_RefillOil".Translate(this.pawn.Named("PAWN")).Resolve(), 828267345);
            }
            Texture2D texture = comp.oilRefillDef.uiIcon;
            Material shaderMaterial = comp.oilRefillDef.graphic.MatSingle;
            Graphics.DrawTexture(iconRect, texture, shaderMaterial);
            GUI.color = Color.white;

            // Draw Progress Bar
            float progress = (float)comp.oilCharges / comp.maxCharges;
            Rect barRect = new (iconRect.xMax + 10, rect.y + 30, rect.width - iconRect.width - 20, 20);
            Texture2D barTex = SolidColorMaterials.NewSolidColorTexture(Color.yellow);
            Widgets.FillableBar(barRect, progress, barTex, SolidColorMaterials.NewSolidColorTexture(Color.black), false);
            
            // Draw Text
            Rect labelRect = new (barRect.x, barRect.yMax + 5, barRect.width, 20);
            Widgets.Label(labelRect, $"{comp.oilCharges} / {comp.maxCharges} Charges");

            // Checkbox for Auto-Refuel (inside the gizmo box)
            rect.xMax -= 26f;
            Rect refuelRect = new (rect.xMax, rect.y + 2f, 24f, 24f);


           // Widgets.DefIcon(refuelRect, comp.oilRefillDef, null, 1f, null, false, null, null, null);
            GUI.DrawTexture(new (refuelRect.x, refuelRect.y, refuelRect.width, refuelRect.height), comp.autoRefuel ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (Widgets.ButtonInvisible(refuelRect, true))
            {
                comp.autoRefuel = !comp.autoRefuel;
                if (comp.autoRefuel)
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }
                else
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                }
            }

            // Handle clicking
            if (Mouse.IsOver(refuelRect))
            {
                Widgets.DrawHighlight(refuelRect);
                string onOff = (comp.autoRefuel ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
                TooltipHandler.TipRegion(refuelRect, () => "DankPyon_AutoOil".Translate(this.pawn.Named("PAWN"), onOff.Named("ONOFF")).Resolve(), 828267346);
            }

            rect.xMax -= 26f;
            Rect oilSetRect = new (rect.xMax, rect.y + 2f, 24f, 24f);
            Widgets.DrawTextureFitted(oilSetRect, OilGizmoIcon, 1);
            //Widgets.DefIcon(oilSetRect, comp.targetOilRefillDef, null, 1f, null, false, comp.targetOilRefillDef.uiIconColor, null, null);
            if (Widgets.ButtonInvisible(oilSetRect, true))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                List<Thing> useableOilCache = [];
                foreach (Thing thing in Utility.GetOils(pawn.inventory))
                {
                    useableOilCache.Add(thing);
                }
                if (useableOilCache.Count > 0)
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    using (List<Thing>.Enumerator enumerator2 = useableOilCache.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Thing drug = enumerator2.Current;
                            string label = drug.def.label.CapitalizeFirst();
                            list.Add(new FloatMenuOption(label, delegate ()
                            {
                                //Insert Logic
                                comp.targetOilRefillDef = drug.def;

                            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                
            }
            
            // Handle clicking
            if (Mouse.IsOver(oilSetRect))
            {
                Widgets.DrawHighlight(oilSetRect);
                TooltipHandler.TipRegion(oilSetRect, () => "DankPyon_AutoOilSelection".Translate(this.pawn.Named("PAWN")).Resolve(), 828267347);
            }

            if (Widgets.ButtonInvisible(iconRect))
            {
                TryRefillOil();
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private void TryRefillOil()
        {
            if (pawn == null || comp == null)
                return;
            // Find oil in inventory
            Thing oilItem = null;
            foreach (Thing item in pawn.inventory.innerContainer)
            {
                if (comp.targetOilRefillDef == item.def)
                {
                    oilItem = item;
                    break;
                }
            }
            if (oilItem == null)
            {
                foreach (Thing item in pawn.inventory.innerContainer)
                {
                    if (comp.oilRefillDef == item.def)
                    {
                        oilItem = item;
                        break;
                    }
                }
            }
            if (oilItem != null && pawn.equipment?.Primary is ThingWithComps weapon)
            {

                Job job = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_UseOil, pawn, oilItem, weapon);
                pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);

                Messages.Message($"Refilled {comp.oilType} for {pawn.LabelShort}'s {comp.parent.LabelShort}.", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message($"No {comp.oilRefillDef.label} available in inventory to refill {comp.parent.LabelShort}.", MessageTypeDefOf.RejectInput);
            }
        }
        public static void DrawBackground(Rect rect, Color colorFactor)
        {
            Color color = GUI.color;
            GUI.color = Widgets.WindowBGFillColor * colorFactor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Widgets.WindowBGBorderColor * colorFactor;
            Widgets.DrawBox(rect, 1, null);
            GUI.color = color;
        }

        public Texture2D BGTexture
        {
            get
            {
                return Command.BGTex;
            }
        }

        public Texture2D BGTextureShrunk
        {
            get
            {
                return Command.BGTexShrunk;
            }
        }

    }

}