using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_PanicAttack : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.Awake())
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (p.Dead)
                return ThoughtState.Inactive;
            Hediff_Anxiety anxiety = p.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety) as Hediff_Anxiety;
            if (anxiety == null)
                return ThoughtState.Inactive;
            if (!anxiety.panic)
                return ThoughtState.Inactive;
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
