using HarmonyLib;
using Verse;

namespace TransparentThings
{
    [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
    public class SavedGameLoaderNow_LoadGameFromSaveFileNow
    {
        public static void Prefix()
        {
            Core.cachedTransparentableThingsByExtensions.Clear();
            Core.cachedTransparentableThingsByMaps.Clear();
        }
    }
}
