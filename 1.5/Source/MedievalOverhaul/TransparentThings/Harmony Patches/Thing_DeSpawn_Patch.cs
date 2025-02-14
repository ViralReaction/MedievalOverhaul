using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace TransparentThings
{
    [HarmonyPatch(typeof(Thing), "DeSpawn")]
    public class Thing_DeSpawn_Patch
    {
        public static bool Prepare()
        {
            return TransparentThingsMod.settings.enableTreeTransparency;
        }
        private static void Prefix(Thing __instance)
        {
            Core.cachedTransparentableThingsByExtensions.Remove(__instance);
            Core.cachedCells.Remove(__instance);
            Map map = __instance.Map;
            List<ThingWithExtension> thingWithExtensionList;
            if (map == null || !__instance.TransparencyAllowed() || !Core.cachedTransparentableThingsByMaps.TryGetValue(map, out thingWithExtensionList) || thingWithExtensionList.RemoveAll((Predicate<ThingWithExtension>)(x => x.thing == __instance)) <= 0)
                return;
            Core.FormTransparencyGridIn(map);
        }
    }
}
