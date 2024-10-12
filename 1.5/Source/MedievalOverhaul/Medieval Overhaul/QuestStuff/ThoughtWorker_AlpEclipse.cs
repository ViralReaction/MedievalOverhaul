using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class ThoughtWorker_AlpEclipse : ThoughtWorker_GameCondition
    {
        public override ThoughtState CurrentStateInternal(Pawn p)
        {
            return base.CurrentStateInternal(p).Active && p.SpawnedOrAnyParentSpawned && !p.PositionHeld.Roofed(p.MapHeld) && !PawnUtility.IsBiologicallyOrArtificiallyBlind(p);
        }
    }
}
