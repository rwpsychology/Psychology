using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_EnactPlot : ThinkNode_JobGiver
    {
        [LogPerformance]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(!pawn.InMentalState)
            {
                return null;
            }
            MentalState_FellPlotting plot = pawn.MentalState as MentalState_FellPlotting;
            if(plot == null)
            {
                return null;
            }
            if(plot.Age < 15000 || plot.target == null || (Rand.Value > 0.05f && !plot.enactingPlot))
            {
                return null;
            }
            plot.enactingPlot = true;
            return new Job(JobDefOf.AttackMelee, plot.target)
            {
                maxNumMeleeAttacks = 500,
                expiryInterval = 50000,
                killIncappedTarget = true,
                canBash = true
            };
        }
    }
}
