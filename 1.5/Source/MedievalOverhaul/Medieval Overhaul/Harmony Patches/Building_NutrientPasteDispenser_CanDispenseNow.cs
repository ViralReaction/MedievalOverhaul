using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

	[HarmonyPatch(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.CanDispenseNow),
		MethodType.Getter)]
	public static class Building_NutrientPasteDispenser_CanDispenseNow
	{
        public static bool Prepare()
        {
            return MedievalOverhaulSettings.settings.slopDispenser;
        }
        [HarmonyPrefix]
		public static bool Prefix(ref bool __result, Building_NutrientPasteDispenser __instance)
		{
			if (__instance is Building_SlopPot slopPot)
			{
				__result = slopPot.CanActuallyDispenseNow;
				return false;
			}
			return true;
		}
	}

}