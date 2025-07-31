using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class QuestFinderUtility
    {
        private static readonly List<QuestScriptDef> possibleQuests;
        private static readonly Dictionary<QuestScriptDef, QuestInformation> extensions;

        static QuestFinderUtility()
        {
            QuestFinderUtility.possibleQuests = new List<QuestScriptDef>();

            foreach (QuestScriptDef def in DefDatabase<QuestScriptDef>.AllDefs)
            {
                if (def.GetModExtension<QuestInformation>() != null)
                {
                    QuestFinderUtility.possibleQuests.Add(def);
                }
            }

            QuestFinderUtility.extensions = new Dictionary<QuestScriptDef, QuestInformation>();

            foreach (QuestScriptDef q in QuestFinderUtility.possibleQuests)
            {
                QuestFinderUtility.extensions[q] = q.GetModExtension<QuestInformation>();
            }
        }
        private static Faction _cachedCultistFaction;
        public static Faction CultistFaction()
        {
            if (_cachedCultistFaction != null) return _cachedCultistFaction;

            // ReSharper disable once TooWideLocalVariableScope
            Faction findFaction;
            for (int i = 0; i < Find.FactionManager.AllFactionsListForReading.Count; i++)
            {
                findFaction = Find.FactionManager.AllFactionsListForReading[i];
                if (findFaction.def != MedievalOverhaulDefOf.DankPyon_ShadowSect) continue;
                _cachedCultistFaction = findFaction;
                return _cachedCultistFaction;
            }
            return null;
        }

        public static IEnumerable<QuestScriptDef> PossibleQuests => (IEnumerable<QuestScriptDef>)QuestFinderUtility.possibleQuests;

        public static QuestInformation FinderInfo(this QuestScriptDef quest) => QuestFinderUtility.extensions[quest];

        public static string Label(this QuestScriptDef quest) => QuestFinderUtility.extensions[quest].label;

        public static string LabelCap(this QuestScriptDef quest) => QuestFinderUtility.extensions[quest].label.CapitalizeFirst();
    }
}
