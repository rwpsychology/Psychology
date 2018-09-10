using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_SurvivedTogether : ThoughtWorker
    {
        [LogPerformance]
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (!RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                return false;
            }
            float time = p.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal);
            if(time >= GenDate.TicksPerYear)
            {
                float otherTime = otherPawn.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal);
                if(time >= (GenDate.TicksPerYear * 6) && otherTime >= (GenDate.TicksPerYear * 6))
                {
                    return ThoughtState.ActiveAtStage(5);
                }
                if (time >= (GenDate.TicksPerYear * 5) && otherTime >= (GenDate.TicksPerYear * 5))
                {
                    return ThoughtState.ActiveAtStage(4);
                }
                if (time >= (GenDate.TicksPerYear * 4) && otherTime >= (GenDate.TicksPerYear * 4))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                if (time >= (GenDate.TicksPerYear * 3) && otherTime >= (GenDate.TicksPerYear * 3))
                {
                    return ThoughtState.ActiveAtStage(2);
                }
                if (time >= (GenDate.TicksPerYear * 2) && otherTime >= (GenDate.TicksPerYear * 2))
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                if (otherTime >= (GenDate.TicksPerYear * 1))
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
