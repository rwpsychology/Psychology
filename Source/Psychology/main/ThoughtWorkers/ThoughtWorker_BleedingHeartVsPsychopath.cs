using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_BleedingHeartVsPsychopath : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!p.RaceProps.Humanlike)
                return (ThoughtState)false;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart))
                return (ThoughtState)false;
            if (!other.RaceProps.Humanlike)
                return (ThoughtState)false;
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
                return (ThoughtState)false;
            if (!other.story.traits.HasTrait(TraitDefOf.Psychopath))
                return (ThoughtState)false;
            return (ThoughtState)true;
        }
    }
}
