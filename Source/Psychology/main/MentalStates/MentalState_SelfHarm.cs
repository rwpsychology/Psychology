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
    public class MentalState_SelfHarm : MentalState
    {

        [LogPerformance]
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (pawn.health != null)
            {

                IEnumerable<BodyPartRecord> parts = (from b in pawn.health.hediffSet.GetNotMissingParts()
                                              where (b.def == BodyPartDefOf.Hand || b.def == BodyPartDefOf.Arm) && b.coverage > 0
                                              select b);
                if (parts.Count() > 0 && pawn.IsHashIntervalTick(2000))
                {
                    foreach (BodyPartRecord part in parts)
                    {
                        if (pawn.health.hediffSet.GetPartHealth(part) > 3 && Rand.Chance(0.08f))
                        {
                            int num = Mathf.Max(3, GenMath.RoundRandom(pawn.health.hediffSet.GetPartHealth(part) * Rand.Range(0.1f, 0.35f)));
                            DamageInfo info = new DamageInfo(DamageDefOf.Cut, num, 0f, -1f, null, part, null);
                            pawn.TakeDamage(info);
                        }
                    }
                }
            }
        }

        public override RandomSocialMode SocialModeMax()
        {
            return 0;
        }
    }
}
