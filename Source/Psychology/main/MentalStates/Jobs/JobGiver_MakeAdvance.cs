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
        [LogPerformance]
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
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
            if (pawn2 == null)
            {
                return null;
            }
            if(Rand.Chance(0.005f) && new InteractionWorker_RomanceAttempt().SuccessChance(pawn2, pawn) > 0f)
            {
                return new Job(JobDefOfPsychology.MakeAdvance, pawn2);
            }
            else if(Rand.Value < 0.5f)
            {
                IntVec3 root = WanderUtility.BestCloseWanderRoot(pawn2.PositionHeld, pawn);
                Func<Pawn, IntVec3, IntVec3, bool> wanderDestValidator = delegate (Pawn p, IntVec3 c, IntVec3 wRoot)
                {
                    Room room = root.GetRoom(p.Map, RegionType.Set_Passable);
                    if (room != null && !WanderRoomUtility.IsValidWanderDest(p, c, root))
                    {
                        return false;
                    }
                    return true;
                };
                return new Job(JobDefOf.Goto, RCellFinder.RandomWanderDestFor(pawn, root, 3f, wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, Danger.Deadly)));
            }
            return null;
        }

        int lastRomanceTick = -9999;
    }
}
