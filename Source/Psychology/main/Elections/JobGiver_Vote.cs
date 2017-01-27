using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Psychology
{
    public class JobGiver_Vote : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 result;
            if (!PartyUtility.TryFindRandomCellInPartyArea(pawn, out result))
            {
                return null;
            }
            if (result.IsValid && result.DistanceToSquared(pawn.Position) < ReachDestDist && result.GetRoom(pawn.Map) == pawn.GetRoom())
            {
                pawn.GetLord().Notify_ReachedDutyLocation(pawn);
                return null;
            }
            return new Job(JobDefOf.Goto, result, 500, true);
        }
        
        private const float ReachDestDist = 50f;
    }
}
