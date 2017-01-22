using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_Pretty : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (RelationsUtility.IsDisfigured(other))
            {
                return false;
            }
            if (pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight) == 0f)
            {
                return false;
            }
            int num = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            if (num == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            if (num == 2)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            return false;
        }
    }
}
