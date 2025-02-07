using HarmonyLib;
using RimWorld;
using System;
using System.Xml;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
    public class BackCompatibile_GetBackCompatibleType
    {

        internal static bool Prefix(string providedClassName, ref Type __result, XmlNode node)
        {
            if (node["stuff"] != null && node["stuff"].InnerText.ToString() == "DankPyon_Log_RawDarkWood")
            {
                node["stuff"].InnerText = "DankPyon_RawDarkWood";
                Log.Message(node["stuff"].InnerText.ToString());
            }
            if (providedClassName == "MedievalOverhaul.Building_WorkTable_HeatPushCustom")
            {
                __result = typeof(Building_WorkTable_HeatPush);
                return false;
            }
            if (providedClassName == "MedievalOverhaul.Building_WorkTableCustom")
            {
                __result = typeof(Building_WorkTable);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public class BackCompatibile_BackCompatibleDefName
    {

        internal static void Postfix(Type defType, ref string __result, bool forDefInjections = false, XmlNode node = null)
        {
            if (defType == typeof(ThingDef))
            {
                if (__result == "DankPyon_Log_RawDarkWood")
                {
                    __result = "DankPyon_RawDarkWood";
                    return;

                }
            }
        }
    }
}