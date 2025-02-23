using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Pawn_InventoryTracker), "GetGizmos")]
    public static class Pawn_InventoryTracker_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn_InventoryTracker __instance)
        {
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }

            if (__instance.pawn.IsColonistPlayerControlled && __instance.pawn.Drafted && __instance.pawn.equipment?.Primary is ThingWithComps weapon)
            {
                CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
                if (oilComp == null || oilComp.oilCharges == 0)
                {
                    List<Thing> useableOilCache = [];
                    foreach (Thing thing in Utility.GetOils(__instance))
                    {
                        useableOilCache.Add(thing);
                    }
                    if (useableOilCache.Count == 0)
                    {
                        yield break;
                    }
                    if (useableOilCache.Count == 1)
                    {
                        Thing thing = useableOilCache[0];
                        yield return new Command_Action
                        {
                            defaultLabel = "Apply Oil",
                            defaultDesc = thing.LabelCapNoCount + ": " + thing.def.description.CapitalizeFirst(),
                            icon = OilGizmoIcon,
                            iconAngle = thing.def.uiIconAngle,
                            iconOffset = thing.def.uiIconOffset,
                            action = () =>
                            {
                                Job job = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_UseOil, __instance.pawn, thing, weapon);
                                __instance.pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                            },
                        };
                    }
                    else
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "TakeDrug".Translate(),
                            defaultDesc = "TakeDrugDesc".Translate(),
                            icon = Pawn_InventoryTracker.DrugTex,
                            action = delegate ()
                            {
                                List<FloatMenuOption> list = new List<FloatMenuOption>();
                                using (List<Thing>.Enumerator enumerator2 = useableOilCache.GetEnumerator())
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        Thing drug = enumerator2.Current;
                                        string label = drug.def.label.CapitalizeFirst();
                                        list.Add(new FloatMenuOption(label, delegate ()
                                        {
                                            FoodUtility.IngestFromInventoryNow(__instance.pawn, drug);
                                        }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                                    }
                                }
                                Find.WindowStack.Add(new FloatMenu(list));
                            }
                        };
                    }
                }
            }

        }

        private static readonly Texture2D OilGizmoIcon = ContentFinder<Texture2D>.Get("UI/Icons/WeaponOil", true);

        
    }
}