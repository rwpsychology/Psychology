using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology
{
    public class MentalState_Paranoia : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        [LogPerformance]
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if(pawn.IsHashIntervalTick(1000) && pawn.Map != null)
            {
                if(Rand.Value < 0.75f)
                {
                    Vector3 pos = pawn.DrawPos + pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation);
                    MoteMaker.ThrowText(pos, pawn.Map, ("ParanoidRambling" + Rand.RangeInclusive(1,33)).Translate(), Color.Lerp(Color.black, Color.red, 0.85f), 3.85f);
                    foreach (Pawn p in pawn.Map.mapPawns.AllPawns)
                    {
                        if(p.RaceProps.Humanlike && p != pawn && (pawn.Position - p.Position).LengthHorizontalSquared <= 36f && GenSight.LineOfSight(pawn.Position, p.Position, pawn.Map, true))
                        {
                            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.HeardParanoia);
                        }
                    }
                }
            }
        }
    }
}
