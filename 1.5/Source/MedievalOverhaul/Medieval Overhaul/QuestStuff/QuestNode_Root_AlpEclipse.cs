using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.QuestGen;

namespace MedievalOverhaul
{
    public class QuestNode_Root_AlpEclipse : QuestNode
    {
        public override bool TestRunInt(Slate slate)
        {
            Map map = QuestGen_Get.GetMap(false, null);
            if (map == null)
            {
                return false;
            }
            return true;
        }

        public override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map = QuestGen_Get.GetMap();
            slate.Set("map", map);
            float points = slate.Get("points", 0f);
            List<Thing> list = new List<Thing>();
            string mainPhaseBeganSignal = QuestGen.GenerateNewSignal("DankPyon_Alp_MainPhaseBegan");
            string mainPhaseEndedSignal = QuestGen.GenerateNewSignal("DankPyon_Alp_MainPhaseEnded");
            string text = QuestGen.GenerateNewSignal("DankPyon_AlpEclipse");
            int num = Mathf.RoundToInt(InitialPhaseDurationDaysRange.RandomInRange * 60000f);
            int delayTicks = Mathf.RoundToInt(MainPhaseDurationDaysRange.RandomInRange * 60000f);
            quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, list, filterDeadPawnsFromLookTargets: false, "[initialPhaseLetterText]", null, "[initialPhaseLetterLabel]");
            quest.GameCondition(MedievalOverhaulDefOf.DankPyon_AlpEclipse, -1, map.Parent, permanent: true, forceDisplayAsDuration: true, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: false);
            quest.Delay(num, delegate
            {
                quest.SignalPass(null, null, mainPhaseBeganSignal);
            }).debugLabel = "Main phase delay";
            quest.AddPart(new QuestPart_AlpDarkness(mainPhaseBeganSignal, map.Parent));
            quest.Letter(LetterDefOf.ThreatBig, mainPhaseBeganSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[mainPhaseLetterText]", null, "[mainPhaseLetterLabel]");
            quest.AddPart(new QuestPart_RandomWaves(mainPhaseBeganSignal, text, 24f, 12f, 36f));
            Faction faction = FactionUtility.DefaultFactionFrom(MedievalOverhaulDefOf.DankPyon_Forest_Faction) != null ? FactionUtility.DefaultFactionFrom(MedievalOverhaulDefOf.DankPyon_Forest_Faction) : null;
            quest.SignalPass(delegate
            {
                int raidPoints = Rand.RangeInclusive((int)(points * 0.4f), (int)(points * 0.6f));
                raidPoints = Math.Min(raidPoints, (int)points);
                points -= raidPoints;
                quest.Raid(map, raidPoints, faction, null, raidArrivalMode: PawnsArrivalModeDefOf.EdgeWalkInGroups, raidStrategy: RaidStrategyDefOf.ImmediateAttack, pawnGroupKind: MedievalOverhaulDefOf.DankPyon_AlpGroup, customLetterLabel: "DankPyon_AlpAttackLetterLabel".Translate(), customLetterText: "DankPyon_AlpAttackLetterText".Translate(), customLetterLabelRules: null, customLetterTextRules: null, walkInSpot: null, tag: null, inSignal: null, rootSymbol: "root", silent: false, canTimeoutOrFlee: false, canSteal: false, canKidnap: false);
            }, text);
            quest.Delay(delayTicks, delegate
            {
                quest.SignalPass(null, null, mainPhaseEndedSignal);
            }, mainPhaseBeganSignal).debugLabel = "Main phase end delay";
            quest.Letter(LetterDefOf.PositiveEvent, mainPhaseEndedSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[darknessLiftedLetterText]", null, "[darknessLiftedLetterLabel]");
            quest.AddPart(new QuestPart_GiveMemoryToHumansOnMap
            {
                memory = MedievalOverhaulDefOf.DankPyon_AlpRaidFinished,
                inSignal = mainPhaseEndedSignal,
                mapParent = map.Parent
            });
            quest.SignalPass(delegate
            {
                quest.End(QuestEndOutcome.Success, 0, null, mainPhaseEndedSignal);
            }, mainPhaseEndedSignal);
            Pawn pawn = map.mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
            if (pawn != null)
            {
                TaleRecorder.RecordTale(MedievalOverhaulDefOf.DankPyon_AlpEclipseTale, pawn);
            }
        }

        private static readonly FloatRange InitialPhaseDurationDaysRange = new FloatRange(0.25f, 0.50f);

        private static readonly FloatRange MainPhaseDurationDaysRange = new FloatRange(2f, 3f);

    }
}
