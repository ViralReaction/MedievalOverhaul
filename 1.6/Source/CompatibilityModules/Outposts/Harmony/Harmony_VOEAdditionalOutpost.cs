using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Outposts;
using Verse;

namespace MedievalOverhaul.Compat
{

    public static class Harmony_VOEAdditionalOutpost
    {
        private static Type VOEAdditionalOutposts_Patches
        {
            get
            {
                return AccessTools.TypeByName("VOEAdditionalOutposts.Outpost_Ranch");
            }
        }
        [HarmonyPatch]
        public static class Harmony_Outpost_Ranch_ResultOptions
        {
            public static bool Prepare()
            {
                return VOEAdditionalOutposts_Patches != null && MedievalOverhaulSettings.settings.leatherChain;
            }

            public static MethodBase TargetMethod()
            {
                return AccessTools.Method(VOEAdditionalOutposts_Patches, "get_ResultOptions");
            }

            public static void Postfix(List<ResultOption> __result)
            {
                if (__result != null && __result.Count > 0)
                {
                    for (int i = 0; i < __result.Count; i++)
                    {
                        ResultOption result = __result[i];
                        if (HideUtility.IsHide(result.Thing))
                        {
                            ThingDefCountClass butcherProduct = result.Thing.butcherProducts[0];
                            result.Thing = butcherProduct.thingDef;
                        }
                    }
                }
            }
        }

    }
}
