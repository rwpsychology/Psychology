using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_CreepyBreathing : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (!other.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
            {
                return false;
            }
            if (pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Hearing) == 0f && pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight) == 0f)
            {
                return false;
            }
            return true;
        }
    }
}
