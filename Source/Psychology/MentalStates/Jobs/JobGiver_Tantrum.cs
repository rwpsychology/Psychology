using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology
{
    public class JobGiver_Tantrum : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (lastTickTantrumed > Find.TickManager.TicksGame - 2000 || Rand.Value < 0.5f)
            {
                return null;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                return !t.IsBurning() && t.def.useHitPoints;
            };
            Building building = (Building)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly, true), 9999f, validator, null, -1, false);
            if (building != null)
            {
                lastTickTantrumed = Find.TickManager.TicksGame;
                int maxNumMeleeAttacks = Rand.RangeInclusive(10, 30) + (Rand.Value < 0.1f ? Rand.RangeInclusive(10,30) : 0);
                pawn.guilt.Notify_Guilty();
                return new Job(JobDefOf.AttackMelee, building)
                {
                    maxNumMeleeAttacks = maxNumMeleeAttacks,
                    expiryInterval = 50000,
                    canBash = true
                };
            }
            return null;
        }

        private int lastTickTantrumed = -9999;
    }
}
