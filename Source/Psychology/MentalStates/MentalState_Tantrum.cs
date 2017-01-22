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
    public class MentalState_Tantrum : MentalState
    {
        private void BruiseFist(Pawn pawn)
        {
            List<BodyPartRecord> hands = (from b in pawn.health.hediffSet.GetNotMissingParts()
                                          where b.def == BodyPartDefOf.LeftHand || b.def == BodyPartDefOf.RightHand
                                          select b).ToList();
            if (hands.Count > 0)
            {
                BodyPartRecord hand = hands.RandomElement();
                int num = Mathf.Max(3, GenMath.RoundRandom(pawn.health.hediffSet.GetPartHealth(hand) * Rand.Range(0.1f, 0.25f)));
                DamageInfo info = new DamageInfo(DamageDefOf.Blunt, num, -1, null, hand, null);
                pawn.TakeDamage(info);
            }
        }

        public override RandomSocialMode SocialModeMax()
        {
            return 0;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if(pawn.jobs != null && pawn.jobs.curJob.def == JobDefOf.AttackMelee && pawn.Position.AdjacentTo8Way(pawn.jobs.curJob.targetA.Thing.Position) && pawn.IsHashIntervalTick(200) && Rand.Value < 0.1f)
            {
                BruiseFist(pawn);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}
