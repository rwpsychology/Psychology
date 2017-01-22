using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Diagnostics;
using UnityEngine;

namespace Psychology
{
    public class Recipe_TreatPyromaniac : Recipe_Surgery
    {
        private bool CheckTreatmentFail(Pawn surgeon, Pawn patient)
        {
            float num = 1f;
            float num2 = surgeon.GetStatValue(StatDefOf.SurgerySuccessChance, true);
            num *= Mathf.Min(num2*2,1f);
            float num3 = surgeon.GetStatValue(StatDefOf.SocialImpact, true);
            num *= num3;
            float num4 = patient.needs.comfort.CurLevel;
            num *= num4;
            if(Rand.Value > num)
            {
                return true;
            }
            return false;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if(!CheckTreatmentFail(billDoer, pawn))
            {
                TaleRecorder.RecordTale(TaleDefOfPsychology.TreatedPyromania, new object[]
                {
                    billDoer,
                    pawn
                });
                if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
                {
                    Messages.Message("TreatedPyromaniac".Translate(new object[] { pawn.LabelShort }), pawn, MessageSound.Benefit);
                }
                Hediff recover = HediffMaker.MakeHediff(HediffDefOfPsychology.RecoveringPyromaniac, pawn, null);
                recover.Tended(1f);
                pawn.health.AddHediff(recover);
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.TreatmentFailed);
        }

        [DebuggerHidden]
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if(pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Pyromaniac) && !pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.RecoveringPyromaniac))
            {
                List<BodyPartRecord> brain = new List<BodyPartRecord>();
                brain.Add(pawn.health.hediffSet.GetBrain());
                return brain;
            }
            return new List<BodyPartRecord>();
        }
    }
}
