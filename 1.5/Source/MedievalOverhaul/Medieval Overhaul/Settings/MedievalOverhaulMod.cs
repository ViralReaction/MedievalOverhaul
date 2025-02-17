using HarmonyLib;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class MedievalOverhaulSettings : Mod
    {
        public static MedievalOverhaul_Settings settings;
        private static ModMetaData _metaData { get; set; }

        public MedievalOverhaulSettings(ModContentPack content) : base(content)
        {
            settings = GetSettings<MedievalOverhaul_Settings>();
            Harmony harmony = new Harmony(id: "medievalOverhaul");
            harmony.PatchAll();
            _metaData = ModLister.GetActiveModWithIdentifier("DankPyon.Medieval.Overhaul", true);
            Log.Message("Medieval Overhaul v " + _metaData.ModVersion);
        }

        public override string SettingsCategory()
        {
            return "MedievalOverhaul.ModNameShort".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
        }

    }
}
