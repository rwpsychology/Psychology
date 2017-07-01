using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Psychology
{
    public class ThoughtWorker_WantToSleepWithSpouseOrLover : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
            if (directPawnRelation == null)
            {
                return false;
            }
            if (!directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() || !directPawnRelation.otherPawn.relations.everSeenByPlayer)
            {
                return false;
            }
            if (p.ownership.OwnedBed != null && p.ownership.OwnedBed == directPawnRelation.otherPawn.ownership.OwnedBed)
            {
                return false;
            }
            if (p.relations.OpinionOf(directPawnRelation.otherPawn) <= 0)
            {
                return false;
            }
            if (p.ownership.OwnedBed != null && p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && p.relations.PotentiallyRelatedPawns.Where(related => LovePartnerRelationUtility.LovePartnerRelationExists(p, related)).Count() > 1 && p.ownership.OwnedBed.GetRoom().ContainedBeds.Where(t => t.AssignedPawns.ToList().Contains(directPawnRelation.otherPawn)).Count() > 0)
            {
                return false;
            }
            return true;
        }
    }
}
