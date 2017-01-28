using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _InteractionWorker_Breakup
    {
        [DetourMethod(typeof(InteractionWorker_Breakup),"Interacted")]
        internal static void _Interacted(this InteractionWorker_Breakup _this, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
			if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
			{
				initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.DivorcedMe, initiator);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.GotMarried);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.GotMarried);
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, initiator);
			}
			else
            {
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
                PsychologyPawn realRecipient = recipient as PsychologyPawn;
                PsychologyPawn realInitiator = initiator as PsychologyPawn;
                if (realRecipient != null && realInitiator != null)
                {
                    AddExLover(realInitiator, realRecipient);
                    AddExLover(realRecipient, realInitiator);
                    AddBrokeUpOpinion(realRecipient, realInitiator);
                    AddBrokeUpMood(realRecipient, realInitiator);
                    AddBrokeUpMood(realInitiator, realRecipient);
                }
                else
                {
                    initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
                    recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.BrokeUpWithMe, initiator);
                    recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
                }
            }
			if (initiator.ownership.OwnedBed != null && initiator.ownership.OwnedBed == recipient.ownership.OwnedBed)
			{
				Pawn pawn = (Rand.Value >= 0.5f) ? recipient : initiator;
				pawn.ownership.UnclaimBed();
			}
			TaleRecorder.RecordTale(TaleDefOf.Breakup, new object[]
			{
				initiator,
				recipient
			});
			if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
			{
				Find.LetterStack.ReceiveLetter("LetterLabelBreakup".Translate(), "LetterNoLongerLovers".Translate(new object[]
				{
					initiator.LabelShort,
					recipient.LabelShort
				}), LetterType.BadNonUrgent, initiator, null);
			}
        }
        
        [DetourMethod(typeof(InteractionWorker_Breakup), "RandomSelectionWeight")]
        internal static float _RandomSelectionWeight(this InteractionWorker_Breakup _this, Pawn initiator, Pawn recipient)
        {
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                return 0f;
            }
            else if(initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                return 0f;
            }
            float chance = 0.02f;
            float romanticFactor = 1f;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if(realInitiator != null)
            {
                chance = 0.05f;
                romanticFactor = Mathf.InverseLerp(1.05f, 0f, realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
            }
            float opinionFactor = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
            float spouseFactor = 1f;
            if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
            {
                spouseFactor = 0.4f;
            }
            return chance * romanticFactor * opinionFactor * spouseFactor;
        }

        [DetourFallback(new string[] { "_RandomSelectionWeight", "_Interacted" })]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursSexual = false;
        }

        private static void AddExLover(PsychologyPawn lover, PsychologyPawn ex)
        {
            PawnRelationDef exLover= new PawnRelationDef();
            exLover.defName = "ExLover" + lover.LabelShort + Find.TickManager.TicksGame;
            exLover.label = "ex-lover";
            exLover.opinionOffset = Mathf.RoundToInt(-15f * lover.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
            exLover.importance = 125f;
            exLover.implied = false;
            exLover.reflexive = false;
            lover.relations.AddDirectRelation(exLover, ex);
        }

        private static void AddBrokeUpOpinion(PsychologyPawn lover, PsychologyPawn ex)
        {
            ThoughtDef brokeUpDef = new ThoughtDef();
            brokeUpDef.defName = "BrokeUpWithMe" + lover.LabelShort + Find.TickManager.TicksGame;
            brokeUpDef.durationDays = 40f;
            brokeUpDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
            ThoughtStage brokeUpStage = new ThoughtStage();
            brokeUpStage.label = "broke up with me";
            brokeUpStage.baseOpinionOffset = Mathf.RoundToInt(-50f * lover.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(5f, 100f, lover.relations.OpinionOf(ex)));
            brokeUpDef.stages.Add(brokeUpStage);
            lover.needs.mood.thoughts.memories.TryGainMemoryThought(brokeUpDef, ex);
        }

        private static void AddBrokeUpMood(PsychologyPawn lover, PsychologyPawn ex)
        {
            ThoughtDef brokeUpMoodDef = new ThoughtDef();
            brokeUpMoodDef.defName = "BrokeUpWithMeMood" + lover.LabelShort + Find.TickManager.TicksGame;
            brokeUpMoodDef.durationDays = 25f;
            brokeUpMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
            ThoughtStage brokeUpStage = new ThoughtStage();
            brokeUpStage.label = "Broke up with {0}";
            brokeUpStage.baseMoodEffect = Mathf.RoundToInt(-20f * Mathf.InverseLerp(0.25f, 0.75f, lover.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic)) * Mathf.InverseLerp(-20f, 100f, lover.relations.OpinionOf(ex)));
            if(brokeUpStage.baseMoodEffect < -5f)
            {
                brokeUpStage.description = "My lover and I parted ways amicably, but it's still a little sad.";
            }
            else
            {
                brokeUpStage.description = "I'm going through a bad break-up right now.";
            }
            brokeUpMoodDef.stages.Add(brokeUpStage);
            if(brokeUpStage.baseMoodEffect > 0f)
            {
                lover.needs.mood.thoughts.memories.TryGainMemoryThought(brokeUpMoodDef, ex);
            }
        }
    }
}
