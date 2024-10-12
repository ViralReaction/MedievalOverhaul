using System;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace MedievalOverhaul
{

    public class QuestPart_AlpDarkness : QuestPart
    {
        public QuestPart_AlpDarkness()
        {
        }

        public QuestPart_AlpDarkness(string inSignal, MapParent mapParent)
        {
            this.inSignal = inSignal;
            this.mapParent = mapParent;
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (this.mapParent.Map == null)
            {
                return;
            }
            if (signal.tag == this.inSignal)
            {
                this.mapParent.Map.gameConditionManager.SetTargetBrightness(0f, 5f);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (this.mapParent.Map != null)
            {
                GameCondition_AlpEclipse activeCondition = this.mapParent.Map.GameConditionManager.GetActiveCondition<GameCondition_AlpEclipse>();
                if (activeCondition != null)
                {
                    activeCondition.Permanent = false;
                    activeCondition.TicksLeft = 301;
                }
                this.mapParent.Map.gameConditionManager.SetTargetBrightness(1f, 5f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.inSignal, "inSignal", null, false);
            Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent", false);
        }

        public string inSignal;

        public MapParent mapParent;
    }
}
