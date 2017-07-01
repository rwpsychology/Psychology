using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalState_Apathy : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Quiet;
        }
    }
}
