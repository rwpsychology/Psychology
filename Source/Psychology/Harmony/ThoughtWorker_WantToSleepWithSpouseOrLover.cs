using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal")]
    public class ThoughtWorker_WantToSleepWithSpouseOrLoverPatch
    {
        [LogPerformance]
        public static void CurrentStateInternal(ref ThoughtState __result, Pawn p)
        {
            if (__result.StageIndex != ThoughtState.Inactive.StageIndex)
            {
                DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
                bool multiplePartners = (from r in p.relations.PotentiallyRelatedPawns
                                         where LovePartnerRelationUtility.LovePartnerRelationExists(p, r)
                                         select r).Count() > 1;
                bool partnerBedInRoom = (from t in p.ownership.OwnedBed.GetRoom().ContainedBeds
                                         where t.AssignedPawns.Contains(directPawnRelation.otherPawn)
                                         select t).Count() > 0;
                if (directPawnRelation != null && p.ownership.OwnedBed != null && p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && multiplePartners && partnerBedInRoom)
                {
                    __result = false;
                }
            }
        }
    }
}
