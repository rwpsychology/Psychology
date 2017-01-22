using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_Light : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Photosensitive))
                return ThoughtState.Inactive;
            if (p.Position == null)
                return ThoughtState.Inactive;
            if (!p.Awake())
                return ThoughtState.Inactive;
            if (p.Map.glowGrid.PsychGlowAt(p.Position) == PsychGlow.Dark)
                return ThoughtState.ActiveAtStage(0);
            if (p.Map.glowGrid.PsychGlowAt(p.Position) == PsychGlow.Overlit)
                return ThoughtState.ActiveAtStage(2);
            return ThoughtState.ActiveAtStage(1);
        }
    }
}
