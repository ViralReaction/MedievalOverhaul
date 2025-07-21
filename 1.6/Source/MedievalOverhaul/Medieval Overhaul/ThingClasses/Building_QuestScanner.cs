using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class Building_QuestScanner : Building
    {

        public bool CanWorkWithoutFuel => refuelableComp == null;
        private CompRefuelable refuelableComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (QuestScriptDefOf.LongRangeMineralScannerLump == null)
            {
                Destroy(DestroyMode.Refund);
                Log.Error("Long Range Mineral Lump Quest has been removed. Destroying and refunding building. ");
            }
            LessonAutoActivator.TeachOpportunity(MedievalOverhaulDefOf.DankPyon_Concept_ExplorerTable, OpportunityType.Important);
            refuelableComp = GetComp<CompRefuelable>();
        }

        public virtual void UsedThisTick()
        {
            refuelableComp?.Notify_UsedThisTick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            BuildableDef cultBook = MedievalOverhaulDefOf.DankPyon_CultBook;
            if (cultBook != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Tome of Knowledge",
                    icon = cultBook.uiIcon,
                    action = () =>
                    {
                        Find.DesignatorManager.Select(new Designator_Build(cultBook));
                    }
                };
            }
            yield break;
        }
    }
}
