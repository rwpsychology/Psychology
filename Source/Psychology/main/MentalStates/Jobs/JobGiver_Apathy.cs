using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_Apathy : JobGiver_Wander
    {
        public JobGiver_Apathy()
        {
            this.wanderRadius = 10f;
            this.ticksBetweenWandersRange = new IntRange(100, 200);
            this.locomotionUrgency = LocomotionUrgency.Amble;
        }
        
        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.Position;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = base.TryGiveJob(pawn);
            if(Rand.Value > 0.05f)
            {
                if (Rand.Value < 0.5f)
                {
                    job.locomotionUrgency = LocomotionUrgency.Walk;
                }
                return job;
            }
            return null;
        }
    }
}
