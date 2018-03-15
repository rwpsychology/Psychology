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
        protected override Job TryGiveJob(Pawn pawn)
        {
            LordToil_HangOut toil = pawn.GetLord().CurLordToil as LordToil_HangOut;
            Pawn friend = (pawn == toil.friends[0] ? toil.friends[1] : toil.friends[0]);
            if (friend == null)
                return null;
            /* Don't give any jobs if they're hungry. It should automatically give them a job to eat through the Duty. */
            if (pawn.needs.food.CurLevel < 0.33f)
            {
                return null;
            }
            if (friend.needs.food.CurLevel < 0.33f)
            {
                return null;
            }
            /* If they are partners, possibly send them to lay down together so they'll do lovin'. */
            if(LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.jobs.curDriver != null && pawn.jobs.curDriver.layingDown == LayingDownState.NotLaying && (pawn.IsHashIntervalTick(GenDate.TicksPerHour) || friend.IsHashIntervalTick(GenDate.TicksPerHour)))
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
            if (pawn.needs.joy.CurLevel < 0.8f && pawn.CanReserve(toil.hangOut.GetTarget(TargetIndex.A), toil.hangOut.def.joyMaxParticipants, 0, null))
            {
                /* Sometimes the joy activity can't be reserved because it's for one person only. */
                if(toil.hangOut.targetA != null)
                    return new Job(toil.hangOut.def, toil.hangOut.targetA);
                return new Job(toil.hangOut.def);
            }
            else if (((pawn.Position - friend.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(pawn.Position, friend.Position, pawn.Map, true)))
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
                IntVec3 cell = pawn.mindState.duty.focus.Cell;
                /* Make sure they only wander within conversational distance. */
                Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false) && (friend.Position - x).LengthHorizontalSquared < 40f && GenSight.LineOfSight(x, friend.Position, pawn.Map, true);
                Room room = cell.GetRoom(pawn.Map, RegionType.Set_Passable);
                if ((from x in room.Cells
                        where validator(x)
                        select x).TryRandomElement(out result))
                {
                    if ((pawn.Position - friend.Position).LengthHorizontalSquared >= 3f || (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.Position != friend.Position))
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
