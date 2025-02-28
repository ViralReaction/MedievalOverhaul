﻿using RimWorld;
using System;
using System.Text;
using Verse;

namespace MedievalOverhaul
{
    public class CompGenericHide : ThingComp
    { 
        public int leatherAmount;
        public ThingDef leatherType;
        public int marketValue;
        public float massValue;

        public CompProperties_GenericHide Props
        {
            get
            {
                return (CompProperties_GenericHide)this.props;
            }
        }

        /// <summary>
        /// Ensures information inside variables is retained after splitting off from a stack
        /// </summary>
        /// <param name="piece">
        /// Is the thing that was split off
        /// </param>
        /// 
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.leatherAmount == 0)
            {
                this.leatherAmount = this.Props.leatherAmount;
            }
        }

        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            CompGenericHide otherComp = piece.TryGetComp<CompGenericHide>();
            if (otherComp != null)
            {
                otherComp.leatherAmount = this.leatherAmount;
                otherComp.marketValue = this.marketValue;
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);
            CompGenericHide otherComp = otherStack.TryGetComp<CompGenericHide>();
            otherComp.leatherAmount = (int)Math.Ceiling(((this.leatherAmount * this.parent.stackCount) + (otherComp.leatherAmount * otherComp.parent.stackCount)) / (double)(this.parent.stackCount + otherStack.stackCount));
            this.leatherAmount = otherComp.leatherAmount;
            var leatherCost = this.Props.leatherType.GetStatValueAbstract(StatDefOf.MarketValue);
            otherComp.marketValue = (int)((int)(leatherAmount * leatherCost) * 0.8f);
            this.marketValue = otherComp.marketValue;
            otherComp.massValue = (leatherAmount * this.Props.leatherType.GetStatValueAbstract(StatDefOf.Mass));
            this.massValue = otherComp.massValue;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref leatherAmount, "leatherAmount");
            Scribe_Defs.Look(ref leatherType, "leatherType");
            Scribe_Values.Look(ref marketValue, "MarketValue");
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Processed leather amount:" + " " + this.leatherAmount);
            if (this.parent.stackCount > 1)
            {
                stringBuilder.AppendWithSeparator("Stack amount:" + " " + this.leatherAmount * this.parent.stackCount, " | ");
            }
            return stringBuilder.ToString();
        }


    }
}
