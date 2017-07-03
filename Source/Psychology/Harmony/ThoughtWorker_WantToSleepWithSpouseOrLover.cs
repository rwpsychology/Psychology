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
        public static void CurrentStateInternal(ref ThoughtState __result, Pawn p)
        {
            DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
            if (directPawnRelation != null && p.ownership.OwnedBed != null && p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && p.relations.PotentiallyRelatedPawns.Where(related => LovePartnerRelationUtility.LovePartnerRelationExists(p, related)).Count() > 1 && p.ownership.OwnedBed.GetRoom().ContainedBeds.Where(t => t.AssignedPawns.ToList().Contains(directPawnRelation.otherPawn)).Count() > 0)
            {
                __result = false;
            }
        }
    }
}
