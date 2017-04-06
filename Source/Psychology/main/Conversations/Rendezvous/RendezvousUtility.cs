using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public static class RendezvousUtility
    {
        public static float ColonySizeFactor(Pawn pawn)
        {
            return Mathf.Clamp01(pawn.Map.mapPawns.FreeColonistsCount / 8f);
        }

        public static float TimeAssignmentFactor(Pawn pawn, int hour)
        {
            if(pawn.timetable.GetAssignment(hour) == TimeAssignmentDefOf.Joy)
            {
                return 1.25f;
            }
            if (pawn.timetable.GetAssignment(hour) == TimeAssignmentDefOf.Sleep)
            {
                return 0.9f;
            }
            if (pawn.timetable.GetAssignment(hour) == TimeAssignmentDefOf.Anything)
            {
                return 0.33f;
            }
            return 0f;
        }
    }
}
