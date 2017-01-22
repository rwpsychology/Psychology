using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Outdoorsy : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy))
                return ThoughtState.Inactive;
            if (p.Position.Roofed(p.Map))
                return ThoughtState.Inactive;
            else
            {
                if (p.Position.GetRoom(p.Map).PsychologicallyOutdoors)
                    return ThoughtState.ActiveAtStage(1);
                return ThoughtState.ActiveAtStage(0);
            }
        }
    }
}
