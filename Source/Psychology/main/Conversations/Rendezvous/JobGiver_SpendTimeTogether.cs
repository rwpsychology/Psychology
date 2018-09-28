using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Reflection;

namespace Psychology
{
    public class JobGiver_SpendTimeTogether : JobGiver_GetJoy
    {
        [LogPerformance]
        protected override Job TryGiveJob(Pawn pawn)
        {
            LordToil_HangOut toil = pawn.GetLord().CurLordToil as LordToil_HangOut;
            Pawn friend = (pawn == toil.friends[0] ? toil.friends[1] : toil.friends[0]);
            if (friend == null)
                return null;
            /* If they are partners, possibly send them to lay down together so they'll do lovin'. */
            if (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.ownership.OwnedBed != null && !pawn.GetPosture().Laying() && (pawn.IsHashIntervalTick(GenDate.TicksPerHour) || friend.IsHashIntervalTick(GenDate.TicksPerHour)))
            {
                return new Job(JobDefOf.LayDown, pawn.ownership.OwnedBed, GenDate.TicksPerHour);
            }
            /* If they have no joy activity assigned, or they've been doing it for 1-3 hours, give them a new one. */
            if (toil.hangOut == null || toil.ticksToNextJoy < Find.TickManager.TicksGame)
            {
                toil.hangOut = base.TryGiveJob(pawn);
                toil.ticksToNextJoy = Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerHour, GenDate.TicksPerHour * 3);
            }
            /* If they need joy, go do the joy activity.*/
            if (toil.hangOut != null && friend.needs.food.CurLevel > 0.33f && pawn.needs.joy.CurLevel < 0.8f)
            {
                /* Sometimes the joy activity can't be reserved because it's for one person only. */
                Job job = new Job(toil.hangOut.def);
                job.targetA = toil.hangOut.targetA;
                job.targetB = toil.hangOut.targetB;
                job.targetC = toil.hangOut.targetC;
                job.targetQueueA = toil.hangOut.targetQueueA;
                job.targetQueueB = toil.hangOut.targetQueueB;
                job.count = toil.hangOut.count;
                job.countQueue = toil.hangOut.countQueue;
                job.expiryInterval = toil.hangOut.expiryInterval;
                job.locomotionUrgency = toil.hangOut.locomotionUrgency;
                if (job.TryMakePreToilReservations(pawn, false))
                    return job;
                else
                    pawn.ClearAllReservations(false);
            }
            if (((pawn.Position - friend.Position).LengthHorizontalSquared >= 54 || !GenSight.LineOfSight(pawn.Position, friend.Position, pawn.Map, true)))
            { /* Make sure they are close to each other if they're not actively doing a joy activity. */
              /* If the other pawn is already walking over, just hang around until they get there. */
                if (friend.CurJob.def != JobDefOf.Goto)
                    return new Job(JobDefOf.Goto, friend);
                else
                {
                    pawn.rotationTracker.FaceCell(friend.Position);
                    return null;
                }
            }
            else
            {
                /* If they are already standing close enough, but can't do the joy activity together, then wander around. */
                IntVec3 result;
                /* Make sure they only wander within conversational distance. */
                Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && x.InAllowedArea(pawn) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false)
                                                                && (x - friend.Position).LengthHorizontalSquared < 50 && GenSight.LineOfSight(x, friend.Position, pawn.Map, true) && x != friend.Position;
                if (CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 12f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => validator(x), null, out result))
                {
                    if ((pawn.Position - friend.Position).LengthHorizontalSquared >= 5 || (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.Position != friend.Position))
                    {
                        /* Sending them to goto a friend ends with them standing right next to/on top of them. So make them respect personal space a little more. */
                        pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
                        if (!result.IsValid || pawn.mindState.nextMoveOrderIsWait)
                        {
                            /* Alternate between relaxing socially and wandering. */
                            pawn.rotationTracker.FaceCell(friend.Position);
                            return null;
                        }
                    }
                    Job wander = new Job(JobDefOf.GotoWander, result);
                    pawn.Map.pawnDestinationReservationManager.Reserve(pawn, wander, result);
                    return wander;
                }
                /* If we can't find a valid spot, just relax socially. */
                pawn.rotationTracker.FaceCell(friend.Position);
                return null;
            }
        }
    }
}
