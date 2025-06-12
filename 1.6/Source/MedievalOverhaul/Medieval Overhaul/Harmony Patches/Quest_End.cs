using HarmonyLib;
using RimWorld;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Quest), "End")]
    public static class Quest_End
    {
        public static void Postfix(Quest __instance, QuestEndOutcome outcome)
        {
            GameComponent_QuestFinder.Instance.Notify_QuestComplete(__instance, outcome);
        }
    }
}
