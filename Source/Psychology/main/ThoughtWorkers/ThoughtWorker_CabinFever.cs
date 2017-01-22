using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_CabinFever : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Downed)
            {
                return ThoughtState.Inactive;
            }
            if (p.HostFaction != null)
            {
                return ThoughtState.Inactive;
            }
            float num = (float)p.needs.mood.recentMemory.TicksSinceOutdoors / 60000f;
            if (num < 2.5f - (p.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy) ? 1f : 0f))
            {
                return ThoughtState.Inactive;
            }
            if (num < 7.5f - (p.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy) ? 2.5f : 0f))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.ActiveAtStage(1);
        }
    }
}
