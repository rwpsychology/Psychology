using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public static class Toils_Sabotage
    {
        public static Toil DoSabotage(TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.FailOnDespawnedOrNull(ind);
            toil.AddFinishAction(delegate
            {
                Building building = (Building)toil.actor.jobs.curJob.GetTarget(ind).Thing;
                if (!building.GetComp<CompBreakdownable>().BrokenDown)
                {
                    building.GetComp<CompBreakdownable>().DoBreakdown();
                }
            });
            return toil;
        }
    }
}
