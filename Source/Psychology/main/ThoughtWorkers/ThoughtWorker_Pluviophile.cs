using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Pluviophile : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Pluviophile))
                return ThoughtState.Inactive;
            if (p.Map.weatherManager.RainRate < 0.25f)
                return ThoughtState.Inactive;
            if (p.Position.Roofed(p.Map))
                return ThoughtState.ActiveAtStage(0);
            return ThoughtState.ActiveAtStage(1);
        }
    }
}
