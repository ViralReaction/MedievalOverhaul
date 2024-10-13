using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Verse;

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
            Scribe_Values.Look(ref refuelableTorch, "refuelableTorch", false);
            Scribe_Values.Look(ref boomalopeTar, "boomalopeTar", false);
            Scribe_Values.Look(ref mealRetexture, "mealRetexture", true);
            Scribe_Values.Look(ref biotechSchematic, "biotechSchematic", false);
            Scribe_Values.Look(ref slopDispenser, "slopDispenser", true);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            Scribe_Collections.Look(ref settingMode, "settingMode", LookMode.Value, LookMode.Value);

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

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.Label((string)"DankPyon_Settings_ProductionChain".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_LeatherChain".Translate(), ref this.leatherChain, "DankPyon_Settings_LeatherChain_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_WoodChain".Translate(), ref this.woodChain, "DankPyon_Settings_WoodChain_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_ClothChain".Translate(), ref this.clothChain, "DankPyon_Settings_ClothChain_Tooltip".Translate());
            listingStandard.Gap();
            listingStandard.GapLine();
            listingStandard.Gap();
            listingStandard.Label((string)"DankPyon_Settings_MapGen".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_IndustrialJunk".Translate(), ref this.industrialJunk, "DankPyon_Settings_IndustrialJunk_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_ExostriderRemains".Translate(), ref this.exostriderRemains, "DankPyon_Settings_ExostriderRemains_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_HornetNest".Translate(), ref this.hornetNest, "DankPyon_Settings_HornetNest_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_VanillaMine".Translate(), ref this.vanillaMine, "DankPyon_Settings_VanillaMine_Tooltip".Translate());
            listingStandard.Gap();
            listingStandard.GapLine();
            listingStandard.Gap();
            listingStandard.Label((string)"DankPyon_Settings_MiscOption".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_SlopDispenser".Translate(), ref this.slopDispenser, "DankPyon_Settings_SlopDispenser_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_RefuelableTorch".Translate(), ref this.refuelableTorch, "DankPyon_Settings_RefuelableTorch_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_BoomalopeTar".Translate(), ref this.boomalopeTar, "DankPyon_Settings_BoomalopeTar_Tooltip".Translate());
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_MealRetexture".Translate(), ref this.mealRetexture, "DankPyon_Settings_MealRetexture_Tooltip".Translate());
            if (ModsConfig.BiotechActive)
            {
                listingStandard.CheckboxLabeled((string)"DankPyon_Settings_SchematicRework".Translate(), ref this.biotechSchematic, "DankPyon_Settings_SchematicRework_Tooltip".Translate());
            }
            listingStandard.GapLine();
            listingStandard.Gap();
            if (listingStandard.ButtonText("Reset to Defaults"))
            {
                ResetSettingsToDefault();
            }
            listingStandard.End();
        }
        public void ResetSettingsToDefault()
        {
            leatherChain = true;
            woodChain = true;
            clothChain = true;

            industrialJunk = false;
            exostriderRemains = false;
            hornetNest = false;
            vanillaMine = false;

            slopDispenser = true;
            refuelableTorch = false;
            boomalopeTar = false;
            mealRetexture = true;

            biotechSchematic = false;

        }
    }
}
