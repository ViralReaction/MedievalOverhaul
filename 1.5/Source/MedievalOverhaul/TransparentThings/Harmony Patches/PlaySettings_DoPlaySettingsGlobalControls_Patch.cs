using HarmonyLib;
using RimWorld;
using Verse;

namespace TransparentThings
{
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public static class PlaySettings_DoPlaySettingsGlobalControls_Patch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            return Core.hasTransparentRoofs;
        }

        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
                return;
            bool makeRoofsSelectable = TransparentThingsMod.settings.makeRoofsSelectable;
            row.ToggleableIcon(ref TransparentThingsMod.settings.makeRoofsSelectable, TexButton.ShowRoofOverlay, (string)"TT.MakeRoofsSelectable".Translate(), SoundDefOf.Mouseover_ButtonToggle);
            if (makeRoofsSelectable == TransparentThingsMod.settings.makeRoofsSelectable)
                return;
            foreach (ThingDef transparentRoof in Core.transparentRoofs)
                transparentRoof.selectable = TransparentThingsMod.settings.makeRoofsSelectable;
        }
    }
}
