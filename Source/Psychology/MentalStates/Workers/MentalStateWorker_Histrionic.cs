using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalStateWorker_Histrionic : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            return !pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent);
        }
    }
}
