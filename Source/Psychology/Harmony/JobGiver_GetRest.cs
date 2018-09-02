using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using System.Reflection;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(JobGiver_GetRest), nameof(JobGiver_GetRest.GetPriority))]
    public static class JobGiver_GetRest_PriorityPatch
    {
        [HarmonyPostfix]
        public static void InsomniacPriority(ref float __result, Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac) && !pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.SleepingPills))
            {
                TimeAssignmentDef timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
                float curLevel = pawn.needs.rest.CurLevel;
                if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
                {
                    if (curLevel < 0.1f)
                    {
                        __result = 1f;
                        return;
                    }
                    __result = 0f;
                    return;
                }
                else if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                {
                    __result = 0f;
                    return;
                }
                else if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                {
                    if (curLevel < 0.1f)
                    {
                        __result = 3f;
                        return;
                    }
                    __result = 0f;
                    return;
                }
                else if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
                {
                    if (curLevel < RestUtility.FallAsleepMaxLevel(pawn)/2f)
                    {
                        __result = 3f;
                        return;
                    }
                    __result = 0f;
                    return;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
