using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MedievalOverhaul
{
    public class MedievalOverhaulSettings : Mod
    {
        public static MedievalOverhaul_Settings settings;
        private static ModMetaData _metaData { get; set; }
        private int selectedTab = 0;
       

        private static readonly string[] TabNames = ["General", "Map Generation", "Misc"];
        public MedievalOverhaulSettings(ModContentPack content) : base(content)
        {
            settings = GetSettings<MedievalOverhaul_Settings>();
            Harmony harmony = new Harmony(id: "medievalOverhaul");
            harmony.PatchAll();
            _metaData = ModLister.GetActiveModWithIdentifier("DankPyon.Medieval.Overhaul", true);
            Log.Message("Medieval Overhaul v " + _metaData.ModVersion);
            selectedTab = settings.lastSelectedTab;
        }

        public override string SettingsCategory()
        {
            return "MedievalOverhaul.ModNameShort".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect tabBarRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
            Rect contentRect = new Rect(inRect.x, inRect.y + 35f, inRect.width, inRect.height - 35f);
            DrawTabs(tabBarRect);
            switch (selectedTab)
            {
                case 0:
                    DrawGeneralSettings(contentRect);
                    break;
                case 1:
                    DrawMapGenerationSettings(contentRect);
                    break;
                case 2:
                    DrawMiscSettings(contentRect);
                    break;
                
            }

        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            Setting_Utility.UpdateSettings();
        }
        private void DrawTabs(Rect rect)
        {
            float tabWidth = rect.width / TabNames.Length;
            for (int i = 0; i < TabNames.Length; i++)
            {
                TextAnchor originalAnchor = Text.Anchor;

                // Center the text
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect tabRect = new Rect(rect.x + (i * tabWidth), rect.y, tabWidth, rect.height);
                if (Widgets.ButtonText(tabRect, TabNames[i], true, true, new Color(0.8f, 0.85f, 1f), true, null))
                {
                    selectedTab = i;
                    settings.lastSelectedTab = i;
                }
                Text.Anchor = originalAnchor;
            }
        }
        private void DrawGeneralSettings(Rect rect)
        {
            settings.DoSettingsWindowContents(rect);
        }
        private void DrawMapGenerationSettings(Rect rect)
        {
            settings.DoSettingsWindowContents_Settlement(rect);
        }

        private void DrawMiscSettings(Rect rect)
        {
            settings.DoSettingsWindowContents_Misc(rect);
        }

    }
}
