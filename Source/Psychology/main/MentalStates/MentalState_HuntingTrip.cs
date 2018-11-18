using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalState_HuntingTrip : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref prey, "prey");
        }

        public Pawn prey = null;
    }
}
