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
				initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.BrokeUpWithMe, initiator);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
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
            float num = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
            float num2 = 1f;
            if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
            {
                num2 = 0.4f;
            }
            return 0.02f * num * num2;
        }

        [DetourFallback(new string[] { "_RandomSelectionWeight", "_Interacted" })]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursSexual = false;
        }
    }
}
