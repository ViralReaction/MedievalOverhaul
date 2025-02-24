using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
    public class JobGiver_ApplyOilWeapon : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (pawn.equipment?.Primary is not ThingWithComps weapon || !weapon.def.IsMeleeWeapon)
            {
                return 0f;
            }
            return 7.5f;
        }

        public override Job TryGiveJob(Pawn pawn)
        {

            if (pawn.equipment?.Primary is ThingWithComps weapon && weapon.def.IsMeleeWeapon)
            {
                if (weapon.TryGetComp<CompWeaponOil>(out var comp))
                {
                    var charges = comp.oilCharges;
                    var autoRefuel = comp.autoRefuel;
                    if (charges == 0 && autoRefuel)
                    {
                        if (pawn.inventory?.innerContainer == null || pawn.inventory.innerContainer.Count == 0)
                        {
                            return null;
                        }

                        Thing oilItem = null;
                        Thing fallbackOilItem = null;
                        foreach (Thing item in pawn.inventory.innerContainer)
                        {
                            if (comp.targetOilRefillDef == item.def)
                            {
                                oilItem = item;
                                break;
                            }
                            else if (fallbackOilItem == null && comp.oilRefillDef == item.def)
                            {
                                fallbackOilItem = item; // Store fallback if we don't find the target oil
                            }
                        }
                        oilItem ??= fallbackOilItem;
                        if (oilItem != null)
                        {
                            Job job = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_UseOil, pawn, oilItem, weapon);
                            job.count = 1;
                            return job;
                        }
                    }
                }
            }
            return null;
        }
    }
}