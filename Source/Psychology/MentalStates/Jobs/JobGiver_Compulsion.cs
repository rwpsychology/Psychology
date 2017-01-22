using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_Compulsion : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.story.WorkTagIsDisabled(WorkTags.Cleaning))
            {
                Thing closestFilth = pawn.Map.listerFilthInHomeArea.FilthInHomeArea.RandomElement();
                if (closestFilth != null && pawn.CanReserveAndReach(closestFilth, PathEndMode.Touch, Danger.Some))
                {
                    return new Job(JobDefOf.Clean, closestFilth);
                }
            }
            if (!pawn.story.WorkTagIsDisabled(WorkTags.Hauling))
            {
                Thing thing = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().RandomElement();
                if (thing != null && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
                {
                    return HaulAIUtility.HaulToStorageJob(pawn, thing);
                }
            }
            return null;
        }
    }
}
