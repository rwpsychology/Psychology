using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalStateWorker_BingingDrugPsychology : MentalStateWorker_BingingDrug
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            return base.StateCanOccur(pawn) && !pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.DrugFree);
        }
    }
}
