using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_Abuse : ThinkNode_JobGiver
    {
        [LogPerformance]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.interactions.InteractedTooRecentlyToInteract() || lastInteractionTick > Find.TickManager.TicksGame - 500)
            {
                return null;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return pawn3 != pawn && !pawn3.Dead && !pawn3.Downed && pawn3.Awake() && InteractionUtility.CanReceiveInteraction(pawn3) && pawn3.RaceProps.Humanlike;
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
            if (pawn2 == null || Rand.Value > 0.5f)
            {
                return null;
            }
            lastInteractionTick = Find.TickManager.TicksGame;
            return new Job(JobDefOfPsychology.Abuse, pawn2);
        }

        private int lastInteractionTick = -9999;
    }
}
