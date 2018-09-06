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

        [LogPerformance]
        public static bool AcceptableGameConditionsToStartHangingOut(Map map)
        {
            if (GatheringsUtility.AnyLordJobPreventsNewGatherings(map))
            {
                return false;
            }
            if (map.dangerWatcher.DangerRating != StoryDanger.None)
            {
                return false;
            }
            int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
            if (freeColonistsSpawnedCount < 4)
            {
                return false;
            }
            int num = 0;
            foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
            {
                if (current.health.hediffSet.BleedRateTotal > 0f)
                {
                    return false;
                }
                if (current.Drafted)
                {
                    num++;
                }
            }
            if ((float)num / (float)freeColonistsSpawnedCount >= 0.5f)
            {
                return false;
            }
            return true;
        }
    }
}
