using System.Linq;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class PlaceWorker_WindMill : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(WindMillUtility.CalculateWindCells(center, rot, def.size).ToList<IntVec3>());
        }
    }
}
