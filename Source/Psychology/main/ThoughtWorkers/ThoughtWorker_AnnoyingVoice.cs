using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_AnnoyingVoice : ThoughtWorker
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
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing) == 0f)
            {
                return false;
            }
            if (other.health.capacities.GetLevel(PawnCapacityDefOf.Talking) == 0f)
            {
                return false;
            }
            //return true;
            return false;
        }
    }
}
