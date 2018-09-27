using System.Collections.Generic;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace Psychology
{
    public class ThoughtWorker_LDR : ThoughtWorker
    {
        [LogPerformance]
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (!RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                return false;
            }
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(p, otherPawn))
            {
                return false;
            }
            if (!PsycheHelper.PsychologyEnabled(p))
            {
                return false;
            }
            if (p.Map == otherPawn.Map)
            {
                PsycheHelper.Comp(p).LDRTick = Find.TickManager.TicksAbs;
                return false;
            }
            int tickSinceLastSeen = PsycheHelper.Comp(p).LDRTick;
            int ticksApart = Find.TickManager.TicksAbs - tickSinceLastSeen;
            int quadrumsApart = Mathf.FloorToInt((float)ticksApart / (float)GenDate.TicksPerQuadrum);
            int maxApart = (p.relations.GetDirectRelation(PawnRelationDefOf.Spouse, otherPawn) == null ? 7 : 6); 
            if (quadrumsApart > maxApart)
            {
                quadrumsApart = maxApart;
            }
            if (quadrumsApart > 1)
            {
                return ThoughtState.ActiveAtStage(quadrumsApart - 1);
            }
            return false;
        }
    }
}
