using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    public class BackCompatibilityInject : Mod
    {
        public BackCompatibilityInject(ModContentPack content) : base(content)
        {
            List<BackCompatibilityConverter> compatibilityConverters =
    AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility),
        "conversionChain");

            compatibilityConverters.Add(new BackCompatibilityConverter_TentWalls());
        }
    }
}
