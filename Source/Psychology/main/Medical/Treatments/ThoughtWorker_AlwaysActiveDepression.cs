using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class ThoughtWorker_AlwaysActiveDepression : ThoughtWorker_AlwaysActive
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if(p.health.hediffSet.HasHediff(HediffDefOfPsychology.Antidepressants))
            {
                return ThoughtState.ActiveAtStage(1);
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
