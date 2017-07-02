using System;
using System.Linq;
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
            if (pawn.needs.food.CurLevel < 0.33f)
            {
                return null;
            }
            if (friend.needs.food.CurLevel < 0.33f)
            {
                return null;
            }
            if(LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.jobs.curDriver != null && pawn.jobs.curDriver.layingDown == LayingDownState.NotLaying && ((pawn.GetHashCode() + friend.GetHashCode()) ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % 24 == 0)
            {
                return new Job(JobDefOf.LayDown, pawn.ownership.OwnedBed);
            }
            if(toil.hangOut == null || toil.ticksToNextJoy < Find.TickManager.TicksGame)
            {
                toil.hangOut = base.TryGiveJob(pawn);
                toil.ticksToNextJoy = Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerHour, GenDate.TicksPerHour * 3);
            }
            if(pawn.needs.joy.CurLevel < 0.8f)
            {
                return toil.hangOut;
            }
            IntVec3 root = WanderUtility.BestCloseWanderRoot(toil.hangOut.targetA.Cell, pawn);
            Func<Pawn, IntVec3, bool> validator = delegate (Pawn wanderer, IntVec3 loc)
            {
                IntVec3 wanderRoot = root;
                Room room = wanderRoot.GetRoom(pawn.Map);
                return room == null || WanderUtility.InSameRoom(wanderRoot, loc, pawn.Map);
            };
            pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
            IntVec3 wanderDest = RCellFinder.RandomWanderDestFor(pawn, root, 5f, validator, PawnUtility.ResolveMaxDanger(pawn, Danger.Some));
            if (!wanderDest.IsValid || pawn.mindState.nextMoveOrderIsWait)
            {
                if ((pawn.Position - friend.Position).LengthHorizontalSquared >= 42f && friend.jobs.curJob.def != JobDefOf.Goto)
                {
                    IntVec3 friendDest = RCellFinder.RandomWanderDestFor(pawn, friend.Position, 5f, validator, PawnUtility.ResolveMaxDanger(pawn, Danger.Some));
                    pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, friendDest);
                    return new Job(JobDefOf.Goto, friendDest);
                }
                return null;
            }
            pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, wanderDest);
            return new Job(JobDefOf.GotoWander, wanderDest);
        }
    }
}
