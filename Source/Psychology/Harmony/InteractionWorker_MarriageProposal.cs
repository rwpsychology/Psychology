using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.AcceptanceChance))]
	public static class InteractionWorker_MarriageProposal_AcceptanceChancePatch
	{
		[HarmonyPrefix]
		public static bool PsychologyException(InteractionWorker_MarriageProposal __instance, ref float __result, Pawn initiator, Pawn recipient)
		{
			if (recipient.GetComp<CompPsychology>() != null && recipient.GetComp<CompPsychology>().isPsychologyPawn)
            {
                float num = 1.2f;
                num *= Mathf.InverseLerp(0f, 0.75f, recipient.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
                if (PsychologyBase.ActivateKinsey())
                {
                    num *= recipient.GetComp<CompPsychology>().Sexuality.AdjustedRomanticDrive;
                }
                num *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
                __result = Mathf.Clamp01(num);
                return false;
				/* If the recipient is a PsychologyPawn, the mod takes over AcceptanceChance for them and the normal method will be ignored. */
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.Interacted))]
	public static class InteractionWorker_MarriageProposal_InteractedPatch
	{
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> BlindfoldedSurgery(IEnumerable<CodeInstruction> instrs)
		{
            //TODO: Kill self.

            int sleepFor = 0;
            int skipNextInstrs = 0;
            float num = 0;
            bool remPsychThoughts = false;
            bool firstBr = true;
            List<Label> removedLabels = new List<Label>();

            foreach(CodeInstruction itr in instrs) {
                /* If they accept a marriage proposal, remove Psychology-generated proposal rejection thoughts. */
                if(sleepFor <= 0 && remPsychThoughts)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InteractionWorker_MarriageProposal_InteractedPatch), nameof(InteractionWorker_MarriageProposal_InteractedPatch.RemovePsychologyThoughts), new Type[] { typeof(Pawn), typeof(Pawn) }));
                    remPsychThoughts = false;
                }
                if (sleepFor > 0)
                    sleepFor -= 1;
                /* Replace vanilla breakup chance. */
                if (skipNextInstrs > 0)
                {
                    removedLabels.AddRange(itr.labels);
                    skipNextInstrs -= 1;
                    if (skipNextInstrs <= 0)
                    {
                        CodeInstruction first = new CodeInstruction(OpCodes.Ldarg_1);
                        first.labels = removedLabels;
                        yield return first;
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InteractionWorker_MarriageProposal_InteractedPatch), nameof(InteractionWorker_MarriageProposal_InteractedPatch.AddPsychRejectedThoughts), new Type[] { typeof(Pawn), typeof(Pawn) }));
                    }
                }
                else
                {

                    if (itr.opcode == OpCodes.Ldc_R4 && float.TryParse("" + itr.operand, out num) && num == 0.4f)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InteractionWorker_MarriageProposal_InteractedPatch), nameof(InteractionWorker_MarriageProposal_InteractedPatch.PsychBreakupChance), new Type[] { typeof(Pawn) }));
                    }
                    else
                    {
                        if (itr.opcode == OpCodes.Ldstr && ("" + itr.operand) == "LetterRejectedProposal")
                        {
                            itr.operand = "LetterRejectedProposalPsychology";
                        }
                        yield return itr;
                    }
                    
                    if (itr.opcode == OpCodes.Ldsfld && itr.operand == AccessTools.Field(typeof(PawnRelationDefOf), nameof(PawnRelationDefOf.Fiance)))
                    {
                        remPsychThoughts = true;
                        sleepFor = 2;
                    }
                    if (itr.opcode == OpCodes.Br && firstBr)
                    {
                        firstBr = false;
                        skipNextInstrs = 16;
                    }
                }
            }
		}

        public static void RemovePsychologyThoughts(Pawn initiator, Pawn recipient)
        {

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
        }

        public static void AddPsychRejectedThoughts(Pawn initiator, Pawn recipient)
        {
            if (PsycheHelper.PsychologyEnabled(initiator))
            {
                ThoughtDef rejectedProposalDef = new ThoughtDef();
                rejectedProposalDef.defName = "RejectedMyProposal" + initiator.LabelShort + Find.TickManager.TicksGame;
                rejectedProposalDef.durationDays = 60f*PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
                rejectedProposalDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
                ThoughtStage rejectedProposalStage = new ThoughtStage();
                rejectedProposalStage.label = "rejected my proposal";
                rejectedProposalStage.baseOpinionOffset = Mathf.RoundToInt(-40f * PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (PsychologyBase.ActivateKinsey() ? PsycheHelper.Comp(initiator).Sexuality.AdjustedRomanticDrive : 1f));
                rejectedProposalDef.stages.Add(rejectedProposalStage);
                ThoughtDef rejectedProposalMoodDef = new ThoughtDef();
                rejectedProposalMoodDef.defName = "RejectedMyProposalMood" + initiator.LabelShort + Find.TickManager.TicksGame;
                rejectedProposalMoodDef.durationDays = 25f;
                rejectedProposalMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
                rejectedProposalMoodDef.stackedEffectMultiplier = 1f;
                ThoughtStage rejectedProposalMoodStage = new ThoughtStage();
                rejectedProposalMoodStage.label = "proposal rejected by {0}";
                rejectedProposalMoodStage.baseMoodEffect = Mathf.RoundToInt(-25f * PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(100f, 5f, initiator.relations.OpinionOf(recipient)));
                if (rejectedProposalMoodStage.baseMoodEffect < -5f)
                {
                    rejectedProposalMoodStage.description = "My lover isn't ready for that kind of commitment right now, and I understand, but rejection is hard to take.";
                }
                else
                {
                    rejectedProposalMoodStage.description = "I can't believe I got turned down. Maybe we're not meant to be together after all?";
                }
                rejectedProposalMoodDef.stages.Add(rejectedProposalMoodStage);
                if (rejectedProposalMoodStage.baseMoodEffect < 0f)
                {
                    initiator.needs.mood.thoughts.memories.TryGainMemory(rejectedProposalMoodDef, recipient);
                }
                initiator.needs.mood.thoughts.memories.TryGainMemory(rejectedProposalDef, recipient);
            }
            else
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RejectedMyProposal, recipient);
            }
            if (PsycheHelper.PsychologyEnabled(recipient))
            {
                ThoughtDef rejectedTheirProposalDef = new ThoughtDef();
                rejectedTheirProposalDef.defName = "IRejectedTheirProposal" + recipient.LabelShort + Find.TickManager.TicksGame;
                rejectedTheirProposalDef.durationDays = 60f * PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
                rejectedTheirProposalDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
                ThoughtStage rejectedTheirProposalStage = new ThoughtStage();
                rejectedTheirProposalStage.label = "I rejected their proposal";
                rejectedTheirProposalStage.baseOpinionOffset = Mathf.RoundToInt(-30f * PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (PsychologyBase.ActivateKinsey() ? 1.75f - PsycheHelper.Comp(recipient).Sexuality.AdjustedRomanticDrive : 1f));
                rejectedTheirProposalDef.stages.Add(rejectedTheirProposalStage);
                ThoughtDef rejectedTheirProposalMoodDef = new ThoughtDef();
                rejectedTheirProposalMoodDef.defName = "IRejectedTheirProposalMood" + recipient.LabelShort + Find.TickManager.TicksGame;
                rejectedTheirProposalMoodDef.durationDays = 25f;
                rejectedTheirProposalMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
                rejectedTheirProposalMoodDef.stackedEffectMultiplier = 1f;
                ThoughtStage rejectedTheirProposalMoodStage = new ThoughtStage();
                rejectedTheirProposalMoodStage.label = "rejected {0}'s proposal";
                rejectedTheirProposalMoodStage.baseMoodEffect = Mathf.RoundToInt(-25f * PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(100f, 5f, recipient.relations.OpinionOf(initiator)));
                if (rejectedTheirProposalMoodStage.baseMoodEffect < -5f)
                {
                    rejectedTheirProposalMoodStage.description = "I wish they wouldn't spring something like that on me.";
                }
                else
                {
                    rejectedTheirProposalMoodStage.description = "I'm not ready for that kind of commitment. If they don't know that, maybe we're not meant to be together after all?";
                }
                rejectedTheirProposalMoodDef.stages.Add(rejectedTheirProposalMoodStage);
                if (rejectedTheirProposalMoodStage.baseMoodEffect < 0f)
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemory(rejectedTheirProposalMoodDef, initiator);
                }
                recipient.needs.mood.thoughts.memories.TryGainMemory(rejectedTheirProposalDef, initiator);
            }
            else
            {
                recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, initiator);
            }
        }

        public static float PsychBreakupChance(Pawn recipient)
        {
            if(recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                /* Codependent pawns will never break up with someone they don't want to marry. */
                return 0f;
            }
            if (PsycheHelper.PsychologyEnabled(recipient))
            {
                /* The less Romantic a pawn is, the more likely they are to break up with someone who asks them to marry them.
                 * If a Pawn's Romantic value is 0.75 or higher, they won't ever do this.
                 */
                return Math.Max(0f, 1f - (1.34f * recipient.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic)));
            }
            else
            {
                return 0.4f;
            }
        }
	}

	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.RandomSelectionWeight), new[] { typeof(Pawn), typeof(Pawn) })]
    public static class InteractionWorker_MarriageProposal_SelectionWeightPatch
    {
        [HarmonyPostfix]
        internal static void _RandomSelectionWeight(InteractionWorker_MarriageProposal __instance, ref float __result, Pawn initiator, Pawn recipient)
        {
            if (PsycheHelper.PsychologyEnabled(initiator))
            {
				if (initiator.gender == Gender.Female)
				{
					/* Undo the effect of this in the postfixed method. */
					__result /= 0.2f;
				}
				__result *= 0.1f + PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) + PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
                if(PsychologyBase.ActivateKinsey())
                {
                    __result *= PsycheHelper.Comp(initiator).Sexuality.AdjustedRomanticDrive;
                }
            }
        }
    }
}
