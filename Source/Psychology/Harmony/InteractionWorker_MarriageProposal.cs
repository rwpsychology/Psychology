using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using System.Reflection;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "AcceptanceChance")]
	public static class InteractionWorker_MarriageProposal_AcceptanceChancePatch
	{
		[HarmonyPrefix]
		public static bool PsychologyException(InteractionWorker_MarriageProposal __instance, ref float __result, Pawn initiator, Pawn recipient)
		{
			PsychologyPawn realRecipient = recipient as PsychologyPawn;
			if (realRecipient != null)
            {
                float num = 1.2f;
                num *= Mathf.InverseLerp(0f, 0.75f, realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
                if (PsychologyBase.ActivateKinsey())
                {
                    num *= realRecipient.sexuality.AdjustedRomanticDrive;
                }
                num *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
                __result = Mathf.Clamp01(num);
                return false;
				/* If the recipient is a PsychologyPawn, the mod takes over AcceptanceChance for them and the normal method will be ignored. */
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "Interacted", new[] { typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) })]
	public static class InteractionWorker_MarriageProposal_InteractedPatch
	{
		[HarmonyPrefix]
		public static bool NewInteracted(InteractionWorker_MarriageProposal __instance, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			//TODO: Turn this into a transpihahaha no.
			float num = __instance.AcceptanceChance(initiator, recipient);
			bool flag = Rand.Value < num;
			bool brokeUp = false;
			if (flag)
			{
				initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, initiator);
				/* Remove custom Psychology rejection thoughts */
				foreach (ThoughtDef d in (from tgt in initiator.needs.mood.thoughts.memories.Memories
										  where tgt.def.defName.Contains("RejectedMyProposal")
										  select tgt.def))
				{
					initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(d, recipient);
				}
				foreach (ThoughtDef d in (from tgt in recipient.needs.mood.thoughts.memories.Memories
										  where tgt.def.defName.Contains("RejectedMyProposal")
										  select tgt.def))
				{
					recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(d, initiator);
				}
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalAccepted);
			}
			else
			{
				PsychologyPawn realInitiator = initiator as PsychologyPawn;
				PsychologyPawn realRecipient = recipient as PsychologyPawn;
				if (realInitiator != null)
				{
					ThoughtDef rejectedProposalDef = new ThoughtDef();
					rejectedProposalDef.defName = "RejectedMyProposal" + realInitiator.LabelShort + Find.TickManager.TicksGame;
					rejectedProposalDef.durationDays = 40f;
					rejectedProposalDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
					ThoughtStage rejectedProposalStage = new ThoughtStage();
					rejectedProposalStage.label = "rejected my proposal";
					rejectedProposalStage.baseOpinionOffset = Mathf.RoundToInt(-30f * realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(100f, 5f, realInitiator.relations.OpinionOf(realRecipient)));
					rejectedProposalDef.stages.Add(rejectedProposalStage);
					ThoughtDef rejectedProposalMoodDef = new ThoughtDef();
					rejectedProposalMoodDef.defName = "RejectedMyProposalMood" + realInitiator.LabelShort + Find.TickManager.TicksGame;
					rejectedProposalMoodDef.durationDays = 25f;
					rejectedProposalMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
					rejectedProposalMoodDef.stackedEffectMultiplier = 1f;
					ThoughtStage rejectedProposalMoodStage = new ThoughtStage();
					rejectedProposalMoodStage.label = "proposal rejected by {0}";
					rejectedProposalMoodStage.baseMoodEffect = Mathf.RoundToInt(-25f * realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(100f, 5f, realInitiator.relations.OpinionOf(realRecipient)));
					if (rejectedProposalMoodStage.baseMoodEffect < -5f)
					{
						rejectedProposalMoodStage.description = "My lover isn't ready for that kind of commitment right now, and I understand, but rejection is hard to take.";
					}
					else
					{
						rejectedProposalMoodStage.description = "I can't believe I got turned down. Maybe we're not meant to be together after all?";
					}
					rejectedProposalMoodDef.stages.Add(rejectedProposalMoodStage);
					if (rejectedProposalMoodStage.baseMoodEffect > 0)
					{
						realInitiator.needs.mood.thoughts.memories.TryGainMemory(rejectedProposalMoodDef, realRecipient);
					}
					realInitiator.needs.mood.thoughts.memories.TryGainMemory(rejectedProposalDef, realRecipient);
				}
				else
				{
					initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RejectedMyProposal, recipient);
				}
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejected);
				if (realRecipient != null && !recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) && Rand.Value > 2f * realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic))
				{
					recipient.interactions.TryInteractWith(initiator, DefDatabase<InteractionDef>.GetNamed("Breakup"));
				}
				else if (realRecipient == null && !recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) && Rand.Value < 0.4f)
				{
					initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
					initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
					brokeUp = true;
					extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp);
				}
			}
			if (initiator.IsColonist || recipient.IsColonist)
			{
				Traverse.Create(__instance).Method("SendLetter", new[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool) }).GetValue(new object[] { initiator, recipient, flag, brokeUp });
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "RandomSelectionWeight", new[] { typeof(Pawn), typeof(Pawn) })]
    public static class InteractionWorker_MarriageProposal_SelectionWeightPatch
    {
        [HarmonyPostfix]
        internal static void _RandomSelectionWeight(InteractionWorker_MarriageProposal __instance, ref float __result, Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realInitiator != null)
            {
				if (initiator.gender == Gender.Female)
				{
					/* Undo the effect of this in the postfixed method. */
					__result /= 0.2f;
				}
				__result *= 0.1f + realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) + realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
                if(PsychologyBase.ActivateKinsey())
                {
                    __result *= realInitiator.sexuality.AdjustedRomanticDrive;
                }
            }
        }
    }
}
