using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_Sadism : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return !pawn3.Dead && !pawn3.Downed && (pawn3.IsPrisoner || pawn3.RaceProps.Animal);
            };
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            if (pawn2 == null || Rand.Value > 0.5f || pawn.mindState.MeleeThreatStillThreat)
            {
                return null;
            }
            return new Job(JobDefOf.AttackMelee, pawn2)
            {
                maxNumMeleeAttacks = 25,
                expiryInterval = 50000,
                killIncappedTarget = false,
                canBash = true
            };
        }
    }
}
