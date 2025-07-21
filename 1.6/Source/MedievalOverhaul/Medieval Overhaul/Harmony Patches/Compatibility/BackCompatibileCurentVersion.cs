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
            if (node["def"] != null && node["def"].InnerText.Contains("DankPyon_HideTentWall"))
            {
                node["def"].InnerText = node["def"].InnerText.Replace("DankPyon_HideTentWall", "DankPyon_TentWall");
            }
            if (node["def"] != null && node["def"].InnerText.Contains("Blueprint_DankPyon_HideTentWall"))
            {
                node["def"].InnerText = node["def"].InnerText.Replace("Blueprint_DankPyon_HideTentWall", "Blueprint_DankPyon_TentWall");
            }
            if (node["def"] != null && node["def"].InnerText.Contains("Frame_DankPyon_HideTentWall"))
            {
                node["def"].InnerText = node["def"].InnerText.Replace("Frame_DankPyon_HideTentWall", "Frame_DankPyon_TentWall");
            }
            return true;
        }
    }
    // [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    // public class BackCompatibile_BackCompatibleDefName
    // {
    //
    //     internal static void Postfix(Type defType, ref string __result, bool forDefInjections = false, XmlNode node = null)
    //     {
    //
    //     }
    //}
}