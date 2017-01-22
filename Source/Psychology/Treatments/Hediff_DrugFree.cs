using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Hediff_DrugFree : Hediff
    {
        public override void PostTick()
        {
            base.PostTick();
            if(pawn.InMentalState && (pawn.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("BingingDrugMajor") || (pawn.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("BingingDrugExtreme"))))
            {
                pawn.MentalState.PostEnd();
            }
        }
    }
}
