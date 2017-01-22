using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Psychology
{
    public class ThoughtWorker_HediffPsychology : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(this.def.hediff);
            if (firstHediffOfDef == null || firstHediffOfDef.def.stages == null)
            {
                return ThoughtState.Inactive;
            }
            if (this.def.defName.Contains("Withdrawal") && p.health.hediffSet.HasHediff(HediffDefOfPsychology.MethadoneHigh))
            {
                return ThoughtState.Inactive;
            }
            int stageIndex = Mathf.Min(new int[]
            {
                firstHediffOfDef.CurStageIndex,
                firstHediffOfDef.def.stages.Count - 1,
                this.def.stages.Count - 1
            });
            return ThoughtState.ActiveAtStage(stageIndex);
        }
    }
}
