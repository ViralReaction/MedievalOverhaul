using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System;

namespace MedievalOverhaul
{
    public class MedievalOverhaul_Settings : ModSettings
    {
        public static Dictionary<string, bool> settingMode = new();

        // Debug Mode
        public bool debugMode = false;

        // Production Chains
        public bool leatherChain = true;
        public bool woodChain = true;
        public bool clothChain = true;

        // Map Generation Stuff
        public bool industrialJunk = false;
        public bool exostriderRemains = false;
        public bool hornetNest = false;
        public bool vanillaMine = false;

        // Misc
        public bool refuelableTorch = false;
        public bool boomalopeTar = false;
        public bool mealRetexture = true;
        public bool biotechSchematic = false;
        public bool slopDispenser = true;
        public bool componentRepair = true;

        //Settlement Generation
        public bool StandaloneSettlementPreference_EnableSettlementPreference = true;
        public bool StandaloneSettlementPreference_Logging = false;
        public bool StandaloneSettlementPreference_LoggingExtended = false;
        public int StandaloneSettlementPreference_Iterations = 50;

        // Experimental Toggles
        public bool experimental_patches = false;
        public bool chemfuel_replace = false;
        public bool component_replace = false;


        public int lastSelectedTab = 0;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref leatherChain, "leatherChain", true);
            Scribe_Values.Look(ref woodChain, "woodChain", true);
            Scribe_Values.Look(ref clothChain, "clothChain", true);
            Scribe_Values.Look(ref industrialJunk, "industrialJunk", false);
            Scribe_Values.Look(ref exostriderRemains, "exostriderRemains", false);
            Scribe_Values.Look(ref hornetNest, "hornetNest", false);
            Scribe_Values.Look(ref vanillaMine, "vanillaMine", false);
            
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            Scribe_Collections.Look(ref settingMode, "settingMode", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref lastSelectedTab, "lastSelectedTab", 0);
            Scribe_Values.Look(ref StandaloneSettlementPreference_EnableSettlementPreference, "StandaloneSettlementPreference_EnableSettlementPreference", true);
            Scribe_Values.Look(ref this.StandaloneSettlementPreference_Logging, "StandaloneSettlementPreference_Logging");
            Scribe_Values.Look(ref this.StandaloneSettlementPreference_LoggingExtended, "StandaloneSettlementPreference_LoggingExtended");
            Scribe_Values.Look(ref this.StandaloneSettlementPreference_Iterations, "StandaloneSettlementPreference_Iterations", 50);

            // Misc Patches
            Scribe_Values.Look(ref refuelableTorch, "refuelableTorch", false);
            Scribe_Values.Look(ref boomalopeTar, "boomalopeTar", false);
            Scribe_Values.Look(ref mealRetexture, "mealRetexture", true);
            Scribe_Values.Look(ref biotechSchematic, "biotechSchematic", false);
            Scribe_Values.Look(ref slopDispenser, "slopDispenser", true);
            Scribe_Values.Look(ref componentRepair, "componentRepair", true);
            //Experimental Patches
            Scribe_Values.Look(ref experimental_patches, "experimental_patches", false);
            Scribe_Values.Look(ref chemfuel_replace, "chemfuel_replace", false);
            Scribe_Values.Look(ref component_replace, "component_replace", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                settingMode ??= new Dictionary<string, bool>();
            }

        }
        public IEnumerable<string> toggleSettings
        {
            get
            {
                return GetType().GetFields().Where(p => p.FieldType == typeof(bool) && (bool)p.GetValue(this)).Select(p => p.Name);
            }
        }
        private Vector2 scrollPosition = Vector2.zero;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            float scrollHeight = 500f;
            Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label((string)"DankPyon_Settings_ProductionChain".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_LeatherChain".Translate(), ref this.leatherChain, "DankPyon_Settings_LeatherChain_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_WoodChain".Translate(), ref this.woodChain, "DankPyon_Settings_WoodChain_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_ClothChain".Translate(), ref this.clothChain, "DankPyon_Settings_ClothChain_Tooltip".Translate());
            listingStandard.Gap();
            listingStandard.GapLine();
            listingStandard.Gap();
            if (listingStandard.ButtonText("Reset to Defaults"))
            {
                ResetSettingsToDefault();
            }
            listingStandard.End();
            Widgets.EndScrollView();
        }

        public void DoSettingsWindowContents_Settlement(Rect inRect)
        {
            float scrollHeight = 500f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label((string)"DankPyon_Settings_MapGen".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_IndustrialJunk".Translate(), ref this.industrialJunk, "DankPyon_Settings_IndustrialJunk_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_ExostriderRemains".Translate(), ref this.exostriderRemains, "DankPyon_Settings_ExostriderRemains_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_HornetNest".Translate(), ref this.hornetNest, "DankPyon_Settings_HornetNest_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_VanillaMine".Translate(), ref this.vanillaMine, "DankPyon_Settings_VanillaMine_Tooltip".Translate());
            listingStandard.Gap();
            listingStandard.GapLine();
            listingStandard.Gap();
            listingStandard.Label((string)"DankPyon_Settings_SettlementGeneration".Translate());
            listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_EnableSettlementPreference".Translate(), ref StandaloneSettlementPreference_EnableSettlementPreference, (string)("StandaloneSettlementPreference_EnableSettlementPreferenceTooltip".Translate() + TooltipStringInit.General_SettlementPreference));
            listingStandard.Gap();
            listingStandard.Label((string)("StandaloneSettlementPreference_Iterations".Translate() + " (") + StandaloneSettlementPreference_Iterations.ToString() + ")", tooltip: ((string)"ESCP_RaceTools_SettlementPreferenceIterationsTooltip".Translate()));
            StandaloneSettlementPreference_Iterations = (int)Math.Round((double)listingStandard.Slider((float)StandaloneSettlementPreference_Iterations, 10f, 5000f) / 10.0) * 10;
            listingStandard.Gap();
            if (Prefs.DevMode)
            {
                listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_Logging".Translate(), ref StandaloneSettlementPreference_Logging, (string)"StandaloneSettlementPreference_LoggingTooltip".Translate());
                listingStandard.Gap();
                listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_LoggingExtended".Translate(), ref StandaloneSettlementPreference_LoggingExtended, (string)"StandaloneSettlementPreference_LoggingExtendedTooltip".Translate());
                listingStandard.Gap();
            }
            listingStandard.GapLine();
            listingStandard.Gap();
            Rect rect = listingStandard.GetRect(30f);
            TooltipHandler.TipRegion(rect, (TipSignal)"StandaloneSettlementPreference_ResetSettings".Translate());
            if (Widgets.ButtonText(rect, (string)"StandaloneSettlementPreference_ResetSettings".Translate()))
            {
                ResetSettings();
            }
            listingStandard.End();
            Widgets.EndScrollView();
        }

        public void DoSettingsWindowContents_Misc(Rect inRect)
        {
            float scrollHeight = 500f;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Custom options = new Listing_Custom();
            options.Begin(inRect);
            options.Gap();
            options.Gap();
            options.CheckboxLabeled((string)"DankPyon_Settings_SlopDispenser".Translate(), ref this.slopDispenser, "DankPyon_Settings_SlopDispenser_Tooltip".Translate());
            options.CheckboxLabeled((string)"DankPyon_Settings_RefuelableTorch".Translate(), ref this.refuelableTorch, "DankPyon_Settings_RefuelableTorch_Tooltip".Translate());
            options.CheckboxLabeled((string)"DankPyon_Settings_BoomalopeTar".Translate(), ref this.boomalopeTar, "DankPyon_Settings_BoomalopeTar_Tooltip".Translate());
            options.CheckboxLabeled((string)"DankPyon_Settings_MealRetexture".Translate(), ref this.mealRetexture, "DankPyon_Settings_MealRetexture_Tooltip".Translate());
            options.CheckboxLabeled((string)"DankPyon_Settings_ComponentRepair".Translate(), ref this.componentRepair, "DankPyon_Settings_ComponentRepair_Tooltip".Translate());
            if (ModsConfig.BiotechActive)
            {
                options.CheckboxLabeled((string)"DankPyon_Settings_SchematicRework".Translate(), ref this.biotechSchematic, "DankPyon_Settings_SchematicRework_Tooltip".Translate());
            }
            options.Gap();
            options.GapLine();
            options.Gap();
            Text.Font = GameFont.Medium;
            options.CheckboxLabeled("DankPyon_Settings_Experimental".Translate(), ref this.experimental_patches, "DankPyon_Settings_Experimental_Desc".Translate());
            Text.Font = GameFont.Small;
            options.Gap();
            if (experimental_patches)
            {
                options.CheckboxLabeled("DankPyon_Settings_Chemfuel_Replace_Title".Translate(), ref this.chemfuel_replace, "DankPyon_Settings_Chemfuel_Replace_Desc".Translate());
                options.CheckboxLabeled("DankPyon_Settings_Component_Replace_Title".Translate(), ref this.component_replace, "DankPyon_Settings_Component_Replace_Desc".Translate());
            }
            options.GapLine();
            options.Gap();
            Rect rect = options.GetRect(30f);
            TooltipHandler.TipRegion(rect, (TipSignal)"StandaloneSettlementPreference_ResetSettings".Translate());
            if (Widgets.ButtonText(rect, (string)"StandaloneSettlementPreference_ResetSettings".Translate()))
            {
                ResetSettingsToDefault_Misc();
            }
            options.End();
            Widgets.EndScrollView();
        }


        public void ResetSettings()
        {
            industrialJunk = false;
            exostriderRemains = false;
            hornetNest = false;
            vanillaMine = false;
            StandaloneSettlementPreference_EnableSettlementPreference = true;
            StandaloneSettlementPreference_Logging = false;
            StandaloneSettlementPreference_LoggingExtended = false;
            StandaloneSettlementPreference_Iterations = 50;
        }

        public void ResetSettingsToDefault()
        {
            leatherChain = true;
            woodChain = true;
            clothChain = true;
        }

        public void ResetSettingsToDefault_Misc()
        {
            slopDispenser = true;
            refuelableTorch = false;
            boomalopeTar = false;
            mealRetexture = true;
            biotechSchematic = false;

            //Experimental Patches
            experimental_patches = false;
            chemfuel_replace = false;
            component_replace = false;

        }
    }
}
