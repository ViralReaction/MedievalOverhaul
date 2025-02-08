using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos
    {
        private static readonly Texture2D OilGizmoIcon = ContentFinder<Texture2D>.Get("UI/Icons/PoisonedOil", true);
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }

            if (__instance.IsColonistPlayerControlled && __instance.equipment?.Primary is ThingWithComps weapon)
            {
                CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
                if (oilComp != null && oilComp.oilCharges > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DankPyon_OilWeaponStatus".Translate(oilComp.oilType, oilComp.oilCharges),
                        defaultDesc = "DankPyon_OilWeaponDesc".Translate(oilComp.oilType, oilComp.oilCharges),
                        icon = OilGizmoIcon,
                        action = () =>
                        {
                            Messages.Message("DankPyon_OilWeaponInfo".Translate(oilComp.oilType, weapon.LabelShort, oilComp.oilCharges), MessageTypeDefOf.NeutralEvent);
                        },
                        defaultIconColor = Color.white
                    };
                }
            }
        }
    }
}