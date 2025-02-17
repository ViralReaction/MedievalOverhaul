using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class PlaceWorker_WindMill : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            IEnumerable<IntVec3> windCellsEnumerable = WindMillUtility.CalculateWindCells(center, rot, def.size);
            List<IntVec3> windCells = [];

            foreach (IntVec3 cell in windCellsEnumerable)
            {
                windCells.Add(cell);
            }

            GenDraw.DrawFieldEdges(windCells);

        }
    }
}
