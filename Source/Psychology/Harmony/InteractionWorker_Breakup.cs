using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.Interacted))]
	public static class InteractionWorker_Breakup_Interacted_Patch
    {
        [LogPerformance]
        [HarmonyPrefix]
		public static bool NewInteracted(InteractionWorker_Breakup __instance, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			/* If you want to patch this method, you can stuff it. */
			Thought thought = __instance.RandomBreakupReason(initiator, recipient);
			if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
			{
				initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DivorcedMe, initiator);
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, initiator);
			}
			else
			{
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
				if (PsycheHelper.PsychologyEnabled(initiator) && PsycheHelper.PsychologyEnabled(recipient))
				{
					BreakupHelperMethods.AddExLover(initiator, recipient);
					//AddExLover(realRecipient, realInitiator);
					BreakupHelperMethods.AddBrokeUpOpinion(recipient, initiator);
					BreakupHelperMethods.AddBrokeUpMood(recipient, initiator);
					BreakupHelperMethods.AddBrokeUpMood(initiator, recipient);
				}
				else
				{
					initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
					recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, initiator);
					recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
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
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("LetterNoLongerLovers", initiator.LabelShort, recipient.LabelShort));
			if (thought != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("FinalStraw", thought.CurStage.label));
			}
			if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
			{
				Find.LetterStack.ReceiveLetter("LetterLabelBreakup".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent, initiator, null);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.RandomSelectionWeight), new[] { typeof(Pawn), typeof(Pawn) })]
	public static class InteractionWorker_RandomSelectionWeight_Patch
    {
        [LogPerformance]
        [HarmonyPrefix]
		public static bool NewSelectionWeight(InteractionWorker_Breakup __instance, ref float __result, Pawn initiator, Pawn recipient)
		{
			/* Also this one. */
			if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				__result = 0f;
				return false;
			}
			else if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
			{
				__result = 0f;
				return false;
			}
			float chance = 0.02f;
			float romanticFactor = 1f;
			if (PsycheHelper.PsychologyEnabled(initiator))
			{
				chance = 0.05f;
				romanticFactor = Mathf.InverseLerp(1.05f, 0f, PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
			}
			float opinionFactor = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
			float spouseFactor = 1f;
			if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
			{
				spouseFactor = 0.4f;
			}
			__result = chance * romanticFactor * opinionFactor * spouseFactor;
			return false;
		}
	}

    internal static class BreakupHelperMethods
    {
        [LogPerformance]
        public static void AddExLover(Pawn lover, Pawn ex)
		{
			/*
             * TODO: Fix the below
             * Just kidding, that's never gonna happen
            PawnRelationDef exLover = new PawnRelationDef();
            exLover.defName = "ExLover" + lover.LabelShort + Find.TickManager.TicksGame;
            exLover.label = "ex-lover";
            exLover.opinionOffset = Mathf.RoundToInt(-15f * lover.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
            exLover.importance = 125f;
            exLover.implied = false;
            exLover.reflexive = false;
            lover.relations.AddDirectRelation(exLover, ex);
            int startTicks = (Current.ProgramState != ProgramState.Playing) ? 0 : Find.TickManager.TicksGame;
            lover.relations.DirectRelations.Add(new DirectPawnRelationDynamic(exLover, ex, startTicks));
            (typeof(Pawn_RelationsTracker).GetField("pawnsWithDirectRelationsWithMe", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex.relations) as HashSet<Pawn>).Add(lover);
            var GainedOrLostDirectRelation = typeof(Pawn_RelationsTracker).GetMethod("GainedOrLostDirectRelation", BindingFlags.Instance | BindingFlags.NonPublic);
            GainedOrLostDirectRelation.Invoke(lover, new object[] { });
            GainedOrLostDirectRelation.Invoke(ex, new object[] { });
             */
			lover.relations.AddDirectRelation(PawnRelationDefOf.ExLover, ex);
		}

        [LogPerformance]
        public static void AddBrokeUpOpinion(Pawn lover, Pawn ex)
		{
			ThoughtDef brokeUpDef = new ThoughtDef();
			brokeUpDef.defName = "BrokeUpWithMe" + lover.LabelShort + Find.TickManager.TicksGame;
			brokeUpDef.durationDays = 40f;
			brokeUpDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
			ThoughtStage brokeUpStage = new ThoughtStage();
			brokeUpStage.label = "broke up with me";
			brokeUpStage.baseOpinionOffset = Mathf.RoundToInt(-50f * PsycheHelper.Comp(lover).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(5f, 100f, lover.relations.OpinionOf(ex)));
			brokeUpDef.stages.Add(brokeUpStage);
			lover.needs.mood.thoughts.memories.TryGainMemory(brokeUpDef, ex);
		}

        [LogPerformance]
        public static void AddBrokeUpMood(Pawn lover, Pawn ex)
		{
			ThoughtDef brokeUpMoodDef = new ThoughtDef();
			brokeUpMoodDef.defName = "BrokeUpWithMeMood" + lover.LabelShort + Find.TickManager.TicksGame;
			brokeUpMoodDef.durationDays = 25f;
			brokeUpMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
			brokeUpMoodDef.stackedEffectMultiplier = 1f;
            brokeUpMoodDef.stackLimit = 999;
            ThoughtStage brokeUpStage = new ThoughtStage();
			brokeUpStage.label = "Broke up with {0}";
			brokeUpStage.baseMoodEffect = Mathf.RoundToInt(-20f * Mathf.InverseLerp(0.25f, 0.75f, PsycheHelper.Comp(lover).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic)) * Mathf.InverseLerp(-20f, 100f, lover.relations.OpinionOf(ex)));
			if (brokeUpStage.baseMoodEffect < -5f)
			{
				brokeUpStage.description = "My lover and I parted ways amicably, but it's still a little sad.";
			}
			else
			{
				brokeUpStage.description = "I'm going through a bad break-up right now.";
			}
			brokeUpMoodDef.stages.Add(brokeUpStage);
			if (brokeUpStage.baseMoodEffect > 0f)
			{
				lover.needs.mood.thoughts.memories.TryGainMemory(brokeUpMoodDef, ex);
			}
		}
	}
}
