using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class CustomWidget
    {

        public static void DrawIntOptionWithButtons(Rect rect, ref int value, string label, int minValue, int maxValue, int spinnerValue)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect(rect.x, rect.y, rect.width - 100f, rect.height);
            Widgets.Label(labelRect, label);
            float buttonWidth = 30f;
            float textWidth = 50f;
            float spacing = 5f;
            float totalWidth = buttonWidth * 2 + textWidth + spacing * 2;
            float availableX = rect.xMax - totalWidth;
            if (Widgets.ButtonText(new Rect(availableX, rect.y, buttonWidth, rect.height), "-"))
            {
                value = Math.Max(value - spinnerValue, minValue);
            }
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(new Rect(availableX + buttonWidth + spacing, rect.y, textWidth, rect.height), ref value, ref buffer, minValue, maxValue);
            if (Widgets.ButtonText(new Rect(availableX + buttonWidth + textWidth + spacing * 2, rect.y, buttonWidth, rect.height), "+"))
            {
                value = Math.Min(value + spinnerValue, maxValue);
            }

            Text.Anchor = anchor;
        }
        public static float HorizontalSlider(Rect rect, float value, float min, float max, bool drawLabel = true, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float newValue = value;
            TextAnchor originalAnchor = Text.Anchor;
            try
            {
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        if (rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                        {
                            GUIUtility.hotControl = controlID;
                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID)
                        {
                            float previousValue = newValue;
                            newValue = Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width * (max - min) + min, min, max);
                            if (roundTo > 0f)
                            {
                                newValue = Mathf.Round(newValue / roundTo) * roundTo;
                            }
                            if (newValue != previousValue)
                            {
                                CustomWidget.CheckPlayDragSliderSound();
                            }

                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                        {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                        break;

                    case EventType.Repaint:
                        Rect sliderRect = new(rect.x, rect.y + (drawLabel ? 20f : 0f), rect.width, 8f); // Adjust position based on label presence
                        Widgets.DrawAtlas(sliderRect, CustomWidget.SliderRailAtlas);

                        float handleX = Mathf.Clamp(rect.x + (newValue - min) / (max - min) * rect.width - 6f, rect.x, rect.x + rect.width - 12f);
                        GUI.DrawTexture(new(handleX, sliderRect.y - 2.5f, 12f, 12f), CustomWidget.SliderHandle);

                        if (drawLabel && !string.IsNullOrEmpty(label))
                        {
                            Text.Anchor = TextAnchor.MiddleCenter;
                            Vector2 labelSize = Text.CalcSize(label);
                            Rect labelRect = new(
                                rect.x + (rect.width / 2f) - (labelSize.x / 2f),
                                rect.y,
                                labelSize.x,
                                labelSize.y
                            );
                            Widgets.Label(labelRect, label);
                        }
                        if (!string.IsNullOrEmpty(leftAlignedLabel))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(new Rect(rect.x, rect.y, rect.width / 2, 20f), leftAlignedLabel);
                        }

                        if (!string.IsNullOrEmpty(rightAlignedLabel))
                        {
                            Text.Anchor = TextAnchor.MiddleRight;
                            Widgets.Label(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, 20f), rightAlignedLabel);
                        }
                        break;
                }

                return newValue;
            }
            finally
            {
                Text.Anchor = originalAnchor;
            }
        }

        public static (float minValue, float maxValue) HorizontalRangeSlider(Rect rect, float minValue, float maxValue, float minLimit, float maxLimit, bool drawLabel = true, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float updatedMinValue = minValue;
            float updatedMaxValue = maxValue;

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        float mouseX = Event.current.mousePosition.x;

                        if (Mathf.Abs(mouseX - (rect.x + (updatedMinValue - minLimit) / (maxLimit - minLimit) * rect.width)) <
                            Mathf.Abs(mouseX - (rect.x + (updatedMaxValue - minLimit) / (maxLimit - minLimit) * rect.width)))
                        {
                            isDraggingMinHandle = true;
                            isDraggingMaxHandle = false;
                        }
                        else
                        {
                            isDraggingMaxHandle = true;
                            isDraggingMinHandle = false;
                        }

                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        float previousMinValue = updatedMinValue;
                        float previousMaxValue = updatedMaxValue;
                        float mouseValue = Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width * (maxLimit - minLimit) + minLimit, minLimit, maxLimit);

                        if (isDraggingMinHandle)
                        {
                            updatedMinValue = Mathf.Clamp(mouseValue, minLimit, updatedMaxValue - 0.01f);
                            if (updatedMinValue != previousMinValue)
                            {
                                CustomWidget.CheckPlayDragSliderSound();
                            }
                        }
                        else if (isDraggingMaxHandle)
                        {
                            updatedMaxValue = Mathf.Clamp(mouseValue, updatedMinValue + 0.01f, maxLimit);
                            if (updatedMaxValue != previousMaxValue)
                            {
                                CustomWidget.CheckPlayDragSliderSound();
                            }
                        }
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && Event.current.button == 0)
                    {
                        isDraggingMinHandle = false;
                        isDraggingMaxHandle = false;
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    float minHandlePos = rect.x + (updatedMinValue - minLimit) / (maxLimit - minLimit) * rect.width;
                    float maxHandlePos = rect.x + (updatedMaxValue - minLimit) / (maxLimit - minLimit) * rect.width;
                    Rect sliderRect = new(rect.x, rect.y + (drawLabel ? 20f : 0f), rect.width, 8f);
                    Widgets.DrawAtlas(sliderRect, CustomWidget.SliderRailAtlas);
                    float handleWidth = 12f;

                    // Add colored rectangle between handles
                    Color originalColor = GUI.color;
                    GUI.color = Color.white;
                    Rect filledAreaRect = new(minHandlePos, sliderRect.y, maxHandlePos - minHandlePos, sliderRect.height);
                    GUI.DrawTexture(filledAreaRect, Texture2D.whiteTexture);
                    GUI.color = originalColor;

                    GUI.DrawTexture(new(minHandlePos - (handleWidth / 2), sliderRect.y - (handleWidth / 2) - 2.5f, handleWidth, handleWidth * 2), CustomWidget.SliderHandle);
                    GUI.DrawTexture(new(maxHandlePos - (handleWidth / 2), sliderRect.y - (handleWidth / 2) - 2.5f, handleWidth, handleWidth * 2), CustomWidget.SliderHandle);
                    if (drawLabel && !string.IsNullOrEmpty(label))
                    {
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Vector2 labelSize = Text.CalcSize(label);
                        Rect labelRect = new(
                            rect.x + (rect.width / 2f) - (labelSize.x / 2f),
                            rect.y,
                            labelSize.x,
                            labelSize.y
                        );
                        Widgets.Label(labelRect, label);
                    }
                    if (!string.IsNullOrEmpty(leftAlignedLabel))
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Rect leftLabelRect = new(rect.x, rect.y, rect.width / 2, 20f);
                        Widgets.Label(leftLabelRect, leftAlignedLabel);
                    }
                    if (!string.IsNullOrEmpty(rightAlignedLabel))
                    {
                        Text.Anchor = TextAnchor.MiddleRight;
                        Rect rightLabelRect = new(rect.x + rect.width / 2, rect.y, rect.width / 2, 20f);
                        Widgets.Label(rightLabelRect, rightAlignedLabel);
                    }
                    break;
            }

            return (updatedMinValue, updatedMaxValue);
        }
        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, bool paintable = false)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Rect rect2 = rect;
            rect2.xMax -= 24f;
            Widgets.Label(rect2, label);
            if (!disabled)
            {
                Widgets.ToggleInvisibleDraggable(rect, ref checkOn, true, paintable);
            }
            Widgets.CheckboxDraw(rect.x + rect.width - 48f, rect.y + (rect.height - 24f) / 2f, checkOn, disabled, 24f, texChecked, texUnchecked);
            Text.Anchor = anchor;
        }

        private static void CheckPlayDragSliderSound()
        {
            if (Time.realtimeSinceStartup > CustomWidget.lastDragSliderSoundTime + 0.075f)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                CustomWidget.lastDragSliderSoundTime = Time.realtimeSinceStartup;
            }
        }
        private static float lastDragSliderSoundTime = -1f;
        private static bool isDraggingMinHandle = false;
        private static bool isDraggingMaxHandle = false;
        private static readonly Texture2D SliderHandle = ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle", true);
        private static readonly Texture2D SliderRailAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/SliderRail", true);

    }
}
