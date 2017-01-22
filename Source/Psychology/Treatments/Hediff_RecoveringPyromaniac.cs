using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Hediff_RecoveringPyromaniac : Hediff
    {
        public override void PostTick()
        {
            base.PostTick();
            if(pawn.InMentalState && pawn.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree"))
            {
                pawn.MentalState.PostEnd();
            }
        }
    }
}
