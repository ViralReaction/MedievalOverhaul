using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Building_QuestScanner : Building
    {

        public bool CanWorkWithoutFuel
        {
            get
            {
                return this.refuelableComp == null;
            }
        }
        private CompRefuelable refuelableComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (QuestScriptDefOf.LongRangeMineralScannerLump == null)
            {
                this.Destroy(DestroyMode.Refund);
                Log.Error("Long Range Mineral Lump Quest has been removed. Destroying and refunding building. ");
            }
            this.refuelableComp = base.GetComp<CompRefuelable>();
        }

        public virtual void UsedThisTick()
        {
            if (this.refuelableComp != null)
            {
                this.refuelableComp.Notify_UsedThisTick();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            BuildableDef def = MedievalOverhaulDefOf.DankPyon_CultBook;
            if (def != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Tome of Knowledge",
                    icon = def.uiIcon,
                    action = () =>
                    {
                        Find.DesignatorManager.Select(new Designator_Build(def));
                    }
                };
            }
            yield break;
        }
    }
}
