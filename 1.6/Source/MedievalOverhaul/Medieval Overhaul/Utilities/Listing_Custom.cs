using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Listing_Custom : Listing_Standard
    {
        public new float verticalSpacing = 5f;
        public void CustomIntBoxWithButtons(string label, ref int intValue, int minValue, int maxValue, int spinnerValue, string tooltip = null, float height = 0f, float labelPct = 1f)
        {
            float height2 = (height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct);
            Rect rect = base.GetRect(height2, labelPct);
            rect.width = Math.Min(rect.width + 24f, base.ColumnWidth);
            if (this.BoundingRectCached == null || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CustomWidget.DrawIntOptionWithButtons(rect, ref intValue, label, minValue, maxValue, spinnerValue);
            }
            base.Gap(this.verticalSpacing);
        }
        public float CustomSliderLabel(string label, float val, float min, float max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string rightLabel = null, string leftLabel = null, float roundTo = -1f)
        {
            Rect rect = base.GetRect(30f, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect.LeftPart(labelPct), label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float result = CustomWidget.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, true, label2, leftLabel, rightLabel, roundTo);
            base.Gap(this.verticalSpacing);
            return result;
        }
        public int CustomSliderLabelInt(string label, int val, int min, int max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string rightLabel = null, string leftLabel = null, float roundTo = -1f)
        {
            Rect rect = base.GetRect(30f, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect.LeftPart(labelPct), label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float result = CustomWidget.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, true, label2, leftLabel, rightLabel, roundTo);
            base.Gap(this.verticalSpacing);
            return (int)result;
        }
        public void CustomCheckboxLabeled(string label, ref bool checkOn, string tooltip = null, float height = 0f, float labelPct = 1f)
        {
            float height2 = (height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct);
            Rect rect = base.GetRect(height2, labelPct);
            rect.width = Math.Min(rect.width + 24f, base.ColumnWidth);
            if (this.BoundingRectCached == null || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CustomWidget.CheckboxLabeled(rect, label, ref checkOn, false, null, null, false, false);
            }
            base.Gap(this.verticalSpacing);
        }
        public (int minValue, int maxValue) CustomSliderLabelIntRange(string label, int minValue, int maxValue, int min, int max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string minLabel = null, string maxLabel = null)
        {
            TextAnchor originalAnchor = Text.Anchor;

            try
            {
                Rect rect = base.GetRect(30f, 1f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect.LeftPart(labelPct), label);
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
                }
                Rect sliderRect = rect.RightPart(1f - labelPct);
                float floatMinValue = minValue;
                float floatMaxValue = maxValue;
                (floatMinValue, floatMaxValue) = CustomWidget.HorizontalRangeSlider(sliderRect, floatMinValue, floatMaxValue, min, max, true, label2, minLabel, maxLabel);
                minValue = Mathf.RoundToInt(floatMinValue);
                maxValue = Mathf.RoundToInt(floatMaxValue);
                return (minValue, maxValue);
            }
            finally
            {
                // Ensure the alignment is always reset, even if an exception occurs
                Text.Anchor = originalAnchor;
            }
        }
        public void CustomDropdownLabeledEnum<T>(string label, ref T selectedValue, Dictionary<T, Action> enumActions, string tooltip = null, string enumValueStarterString = "DankPyon_Settings", float height = 0f, float labelPct = 1f, float dropdownWidthFactor = 0.75f) where T : Enum
        {
            float height2 = (height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct);
            Rect rect = base.GetRect(height2, labelPct);
            float labelWidth = rect.width * 0.4f;
            float dropdownWidth = rect.width * dropdownWidthFactor;
            float rightPadding = 10f;
            Rect dropdownRect = new Rect(rect.xMax - dropdownWidth - rightPadding, rect.y, dropdownWidth, rect.height);
            Rect labelRect = new Rect(rect.x, rect.y, rect.width - dropdownWidth - rightPadding, rect.height);
            if (this.BoundingRectCached == null || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(dropdownRect))
                    {
                        Widgets.DrawHighlight(dropdownRect);
                    }
                    TooltipHandler.TipRegion(dropdownRect, tooltip);
                }
                TextAnchor originalAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, label);
                Text.Anchor = originalAnchor;
                string translatedSelectedValue = (enumValueStarterString + selectedValue.ToString()).Translate();
                if (Widgets.ButtonText(dropdownRect, translatedSelectedValue))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (T enumValue in Enum.GetValues(typeof(T)))
                    {
                        options.Add(new FloatMenuOption(enumValue.ToString(), () =>
                        {
                            if (enumActions != null && enumActions.ContainsKey(enumValue))
                            {
                                enumActions[enumValue]?.Invoke();
                            }
                            else
                            {
                                Log.Warning("No action defined for: " + enumValue.ToString());
                            }
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
            base.Gap(this.verticalSpacing);
        }

    }
}
