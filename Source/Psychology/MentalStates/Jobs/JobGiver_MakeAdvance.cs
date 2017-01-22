using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_MakeAdvance : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.interactions.InteractedTooRecentlyToInteract() || lastRomanceTick > Find.TickManager.TicksGame - 1000)
            {
                return null;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return pawn3 != pawn && pawn3.Spawned && !pawn3.Dead && !pawn3.Downed && pawn3.Awake() && pawn3.IsColonist;
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            if (pawn2 == null)
            {
                return null;
            }
            if(Rand.Value > 0.005f)
            {
                return new Job(JobDefOfPsychology.MakeAdvance, pawn2);
            }
            else if(Rand.Value < 0.5f)
            {
                return new Job(JobDefOf.Goto, pawn2);
            }
            return null;
        }

        int lastRomanceTick = -9999;
    }
}
