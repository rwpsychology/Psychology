using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace Psychology
{
    public class JobDriver_Abuse : JobDriver
    {
        private static Toil AbuseTarget(Pawn target)
        {
            Toil toil = new Toil();
            toil.AddFailCondition(() => target == null || target.Destroyed || target.Downed || !target.Spawned || target.Dead);
            toil.socialMode = 0;
            toil.initAction = delegate
            {
                Pawn actor = toil.GetActor();
                if (Rand.Value < 0.3f)
                {
                    actor.interactions.TryInteractWith(target, InteractionDefOf.Insult);
                }
                else
                {
                    actor.interactions.TryInteractWith(target, DefDatabase<InteractionDef>.GetNamed("Slight"));
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
            yield return JobDriver_Abuse.ReachTarget(target);
            yield return JobDriver_Abuse.AbuseTarget(target);
            yield break;
        }
    }
}
