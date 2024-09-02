using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Comp_TrollRegen : ThingComp
    {
        private bool fireFlag = false;
        public CompProperties_TrollRegen Props => (CompProperties_TrollRegen)this.props;
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.HasAttachment(ThingDefOf.Fire))
            {
                fireFlag = true;
            }
            if (this.parent.IsHashIntervalTick(Props.tickInterval) && !fireFlag && this.parent is Pawn pawn && pawn.health?.hediffSet.hediffs is { Count: > 0 } hediffs)
            {
                var injuryList = hediffs.OfType<Hediff_Injury>().ToList();
                if (injuryList.Count > 0)
                {
                    var hurt = injuryList.RandomElement();
                    hurt.Severity -= Props.healAmount;
                }
            }
            if (this.parent.IsHashIntervalTick(Props.tickRegenBurn) && fireFlag && !this.parent.HasAttachment(ThingDefOf.Fire))
            {
                fireFlag = false;
            }
        }
    }
}