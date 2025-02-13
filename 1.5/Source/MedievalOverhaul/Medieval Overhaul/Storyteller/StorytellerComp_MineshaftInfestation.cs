using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class StorytellerComp_MineshaftInfestation : StorytellerComp
    {
        private static List<Thing> tmpDrills = new List<Thing>();

        protected StorytellerCompProperties_MineshaftInfestation Props => (StorytellerCompProperties_MineshaftInfestation)props;
        public override void Initialize()
        {
            if (MedievalOverhaulDefOf.DankPyon_MineShaft == null)
            {
                Log.Error("Error: DankPyon_MineShaft is missing from Medieval Overhaul. Another mod may have removed it while leaving StorytellerComp_MineshaftInfestation intact, which could cause issues. This component requires the missing definition.");
            }
        }
        private float DeepDrillInfestationMTBDaysPerDrill
        {
            get
            {
                Difficulty difficulty = Find.Storyteller.difficulty;
                if (difficulty.deepDrillInfestationChanceFactor <= 0f)
                {
                    return -1f;
                }
                return Props.baseMtbDaysPerDrill / difficulty.deepDrillInfestationChanceFactor;
            }
        }

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            Map map = (Map)target;
            tmpDrills.Clear();
            Utility.GetUsableDeepDrills(map, tmpDrills);
            if (!tmpDrills.Any())
            {
                yield break;
            }
            float mtb = DeepDrillInfestationMTBDaysPerDrill;
            if (mtb < 0f)
            {
                yield break;
            }
            for (int i = 0; i < tmpDrills.Count; i++)
            {
                if (Rand.MTBEventOccurs(mtb, 60000f, 1000f))
                {
                    IncidentParms parms = GenerateParms(IncidentCategoryDefOf.DeepDrillInfestation, target);
                    if (UsableIncidentsInCategory(IncidentCategoryDefOf.DeepDrillInfestation, parms).TryRandomElement(out var result))
                    {
                        yield return new FiringIncident(result, this, parms);
                    }
                }
            }
        }
    }

}