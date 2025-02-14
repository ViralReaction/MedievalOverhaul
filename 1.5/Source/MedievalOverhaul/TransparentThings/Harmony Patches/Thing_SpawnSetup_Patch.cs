using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TransparentThings
{
    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public class Thing_SpawnSetup_Patch
    {
        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }
        private static void Postfix(Thing __instance)
        {
            //if (__instance.def.plant?.IsTree == true)
            //{
            //    TreeGrid.UpdateTree(__instance.Position, false);
            //}
            //if (!__instance.TransparencyAllowed())
            //    return;
            //ThingExtension modExtension = __instance.def.GetModExtension<ThingExtension>();
            //if (modExtension != null && (modExtension.transparentWhenItemIsInsideArea || modExtension.transparentWhenPawnIsInsideArea))
            //{
            //    Map map = __instance.Map;
            //    Core.cachedTransparentableThingsByExtensions[__instance] = modExtension;
            //    List<ThingWithExtension> list;
            //    if (!Core.cachedTransparentableThingsByMaps.TryGetValue(map, out list))
            //        Core.cachedTransparentableThingsByMaps[__instance.Map] = list = new List<ThingWithExtension>();
            //    if (!list.Any<ThingWithExtension>((Predicate<ThingWithExtension>)(x => x.thing == __instance)))
            //        list.Add(new ThingWithExtension()
            //        {
            //            thing = __instance,
            //            extension = __instance.def.GetModExtension<ThingExtension>()
            //        });
            //    Core.FormTransparencyGridIn(map);
            //}
        }
    }
}
