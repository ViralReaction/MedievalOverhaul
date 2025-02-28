using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using VOE;
using Outposts;
using Verse;

namespace MedievalOverhaul.Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("medievalOverhaul.compat");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(Outpost_Hunting), nameof(Outpost_Hunting.ResultOptions), MethodType.Getter)]
    public class Outpost_Hunting_ResultOptions
    {
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.leatherChain;
        }
        public static List<ResultOption> Postfix(List<ResultOption> __result)
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
            return __result;
        }
    }
}
