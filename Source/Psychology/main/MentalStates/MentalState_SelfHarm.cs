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

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (pawn.health != null)
            {

                List<BodyPartRecord> parts = (from b in pawn.health.hediffSet.GetNotMissingParts()
                                              where b.def == BodyPartDefOf.LeftHand || b.def == BodyPartDefOf.RightHand || b.def == BodyPartDefOf.LeftArm || b.def == BodyPartDefOf.RightArm
                                              select b).ToList();
                if (parts.Count > 0 && pawn.IsHashIntervalTick(2000))
                {
                    foreach (BodyPartRecord part in parts)
                    {
                        if (pawn.health.hediffSet.GetPartHealth(part) > 3 && Rand.Value < 0.25f)
                        {
                            int num = Mathf.Max(3, GenMath.RoundRandom(pawn.health.hediffSet.GetPartHealth(part) * Rand.Range(0.1f, 0.35f)));
                            DamageInfo info = new DamageInfo(DamageDefOf.Cut, num, -1, null, part, null);
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
