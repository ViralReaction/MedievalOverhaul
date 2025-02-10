using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public static class WindMillUtility
    {
        public static IEnumerable<IntVec3> CalculateWindCells(IntVec3 center, Rot4 rot, IntVec2 size)
        {
            CellRect rectA, rectB;
            int forwardLength, backwardLength;

            // Make forward wind path **equal to windmill size** (instead of 3x)
            if (rot == Rot4.North || rot == Rot4.South)
            {
                forwardLength = size.z;
                backwardLength = Math.Max(1, size.z / 2);  // Ensure at least 1 cell
            }
            else
            {
                forwardLength = size.z;
                backwardLength = Math.Max(1, size.z / 2);
            }

            // Calculate affected cell areas based on rotation
            if (rot == Rot4.North)
            {
                rectA = new CellRect(center.x - size.x / 2, center.z + size.z / 2 + 1, size.x, forwardLength);
                rectB = new CellRect(center.x - size.x / 2, center.z - size.z / 2 - backwardLength + 1, size.x, backwardLength);
            }
            else if (rot == Rot4.South)
            {
                rectA = new CellRect(center.x - size.x / 2 , center.z - size.z / 2 - forwardLength, size.x, forwardLength);
                rectB = new CellRect(center.x - size.x / 2, center.z + size.z / 2, size.x, backwardLength);
            }
            else if (rot == Rot4.East)
            {
                rectA = new CellRect(center.x + size.x / 2 + 2, center.z - size.z / 2 + 1, forwardLength, size.x);
                rectB = new CellRect(center.x - size.x / 2 - backwardLength, center.z - size.z / 2 + 1, backwardLength, size.x);
            }
            else // Rot4.West
            {
                rectA = new CellRect(center.x - size.x / 2 - forwardLength - 1, center.z - size.z / 2 + 1, forwardLength, size.x);
                rectB = new CellRect(center.x + size.x / 2 + 1, center.z - size.z / 2 + 1, backwardLength, size.x);
            }

            // Yield wind cells in front and behind the windmill
            foreach (IntVec3 cell in rectA.Cells) yield return cell;
            foreach (IntVec3 cell in rectB.Cells) yield return cell;
        }
    }
}
