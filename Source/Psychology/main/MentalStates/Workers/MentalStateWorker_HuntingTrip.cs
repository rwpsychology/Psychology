using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalStateWorker_HuntingTrip : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            return pawn.Map != null && !pawn.IsPrisoner && !pawn.story.WorkTagIsDisabled(WorkTags.Violent) && pawn.workSettings.WorkIsActive(WorkTypeDefOf.Hunting);
        }
    }
}
