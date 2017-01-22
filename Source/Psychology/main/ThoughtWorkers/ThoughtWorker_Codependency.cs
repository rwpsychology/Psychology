using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Codependency : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
                return ThoughtState.Inactive;
            if (!LovePartnerRelationUtility.HasAnyLovePartner(p))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else
            {
                Pawn lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                if (lover == null)
                {
                    lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    if (lover == null)
                        lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                    if (lover == null)
                        throw new NotImplementedException();
                    if(lover.Dead == false)
                        return ThoughtState.ActiveAtStage(2);
                    else
                        return ThoughtState.ActiveAtStage(3);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(1);
                }
            }
        }
    }
}
