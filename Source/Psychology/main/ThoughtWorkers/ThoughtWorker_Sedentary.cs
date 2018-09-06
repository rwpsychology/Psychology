using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Sedentary : ThoughtWorker
    {
        [LogPerformance]
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.Awake())
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.Sedentary))
                return ThoughtState.Inactive;
            if ((this.lastMovePosition - p.Position).LengthHorizontalSquared > 80f)
            {
                this.lastMovePosition = p.Position;
                this.lastMoveTick = Find.TickManager.TicksGame;
            }
            if ((Find.TickManager.TicksGame - lastMoveTick) > GenDate.TicksPerHour)
            {
                Building edifice = GridsUtility.GetEdifice(p.Position, p.Map);
                if (edifice != null && edifice.GetStatValue(StatDefOf.Comfort, true) >= 0.75f)
                    return ThoughtState.ActiveAtStage(1);
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }

        private IntVec3 lastMovePosition = new IntVec3();
        private int lastMoveTick = 0;
    }
}
