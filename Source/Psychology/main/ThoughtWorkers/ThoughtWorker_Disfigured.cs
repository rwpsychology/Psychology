using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_Disfigured : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || other.Dead)
            {
                return false;
            }
            if (!RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (!RelationsUtility.IsDisfigured(other))
            {
                return false;
            }
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) == 0f)
            {
                return false;
            }
            if (pawn is PsychologyPawn)
            {
                return false;
            }
            return true;
        }
    }
}
