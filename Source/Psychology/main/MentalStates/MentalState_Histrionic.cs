using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalState_Histrionic : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.SuperActive;
        }

        [LogPerformance]
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.RebuffedMyRomanceAttempt);
        }
    }
}
