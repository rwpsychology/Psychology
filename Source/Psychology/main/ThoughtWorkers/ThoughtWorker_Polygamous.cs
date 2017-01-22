using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_Polygamous : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                return ThoughtState.Inactive;
            if (!LovePartnerRelationUtility.HasAnyLovePartner(p))
            {
                return ThoughtState.Inactive;
            }
            List<Pawn> lovers = new List<Pawn>();
            List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
            foreach(DirectPawnRelation rel in directRelations)
            {
                if(LovePartnerRelationUtility.IsLovePartnerRelation(rel.def) && !rel.otherPawn.Dead)
                {
                    lovers.Add(rel.otherPawn);
                }
            }
            if(lovers.Count == 1 && !lovers[0].story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}
