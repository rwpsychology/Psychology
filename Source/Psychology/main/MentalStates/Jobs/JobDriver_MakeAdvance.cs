using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobDriver_MakeAdvance : JobDriver
    {
        private static Toil FlirtWithTarget(Pawn target)
        {
            Toil toil = new Toil();
            toil.AddFailCondition(() => target == null || target.Destroyed || target.Downed || !target.Spawned || target.Dead);
            toil.socialMode = 0;
            toil.initAction = delegate
            {
                Pawn actor = toil.GetActor();
                if (actor.relations.DirectRelationExists(PawnRelationDefOf.Lover, target))
                {
                    actor.interactions.TryInteractWith(target, InteractionDefOf.MarriageProposal);
                }
                else
                {
                    actor.interactions.TryInteractWith(target, InteractionDefOf.RomanceAttempt);
                }
            };
            return toil;
        }

        private static Toil ReachTarget(Pawn target)
        {
            Toil toil = new Toil();
            toil.AddFailCondition(() => target == null || target.Destroyed || target.Downed || !target.Spawned || target.Dead);
            toil.socialMode = 0;
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.initAction = delegate
            {
                toil.GetActor().pather.StartPath(target, PathEndMode.Touch);
            };
            return toil;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedOrNull(this, TargetIndex.A);
            ToilFailConditions.FailOnDowned(this, TargetIndex.A);
            Pawn target = this.TargetA.Thing as Pawn;
            yield return JobDriver_MakeAdvance.ReachTarget(target);
            yield return JobDriver_MakeAdvance.FlirtWithTarget(target);
            yield break;
        }
    }
}
