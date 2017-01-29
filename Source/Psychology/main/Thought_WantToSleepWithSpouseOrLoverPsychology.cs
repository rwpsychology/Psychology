using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class Thought_WantToSleepWithSpouseOrLoverPsychology : Thought_WantToSleepWithSpouseOrLover
    {
        public override string LabelCap
        {
            get
            {
                if(this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                {
                    return string.Format(base.CurStage.label, "my partners").CapitalizeFirst();
                }
                DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(this.pawn, false);
                return string.Format(base.CurStage.label, directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
            }
        }
    }
}
