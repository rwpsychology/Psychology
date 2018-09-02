using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalState_HuntingTrip : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            /* Prevent pawns from carrying their kills back to the colony. */
            if(this.pawn.CurJob.haulMode == HaulMode.ToCellStorage)
            {
                this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                hunting = null;
            }
            /* Prevent players from undesignating things the pawn wants to hunt. */
            if (hunting == null && this.pawn.CurJobDef == JobDefOf.Hunt)
            {
                hunting = this.pawn.CurJob.targetA.Thing as Pawn;
            }
            if (hunting != null && (hunting.Dead || hunting.Map != this.pawn.Map))
            {
                hunting = null;
            }
            if (hunting != null && (pawn.CurJob.targetA != hunting || pawn.Map.designationManager.DesignationOn(hunting, DesignationDefOf.Hunt) == null))
            {
                /* Out of courtesy, remove the designation on whatever they were just hunting so that other colonists don't try to hunt it. */
                if (pawn.Map.designationManager.DesignationOn(pawn.CurJob.targetA.Thing, DesignationDefOf.Hunt) != null)
                {
                    hunting.Map.designationManager.TryRemoveDesignationOn(pawn.CurJob.targetA.Thing, DesignationDefOf.Hunt);
                }
                if (pawn.Map.designationManager.DesignationOn(hunting, DesignationDefOf.Hunt) == null)
                {
                    Designation hunt = new Designation(hunting, DesignationDefOf.Hunt);
                    hunting.Map.designationManager.AddDesignation(hunt);
                }
                this.pawn.jobs.StartJob(new Job(JobDefOf.Hunt, hunting), JobCondition.InterruptForced);
            }
        }

        Pawn hunting = null;
    }
}
