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
    public class JobDriver_BreakHunt : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.TargetA;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedOrNull(this, TargetIndex.A);
            Pawn prey = this.TargetA.Thing as Pawn;
            if (prey.Dead)
            {
                this.EndJobWith(JobCondition.Succeeded);
            }
            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, true);
            yield return Toils_Combat.CastVerb(TargetIndex.A);
            yield break;
        }
    }
}
