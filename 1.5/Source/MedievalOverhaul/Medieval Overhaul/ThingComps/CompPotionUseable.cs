using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class CompUseEffect_KnowledgePotion : CompUseEffect
    {

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            int skillCount = usedBy.skills.skills.Count - 1;
            var randomNum = Rand.RangeInclusive(0, skillCount);
            usedBy.skills.skills[randomNum].Level += 1;
        }

    }
}
