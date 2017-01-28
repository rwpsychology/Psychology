using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _InteractionWorker_MarriageProposal
    {
        [DetourMethod(typeof(InteractionWorker_MarriageProposal), "AcceptanceChance")]
        internal static float _AcceptanceChance(this InteractionWorker_MarriageProposal _this, Pawn initiator, Pawn recipient)
        {
            float num = 1.2f;
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            if(realRecipient != null)
            {
                num *= Mathf.InverseLerp(0f, 0.75f, realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
                if (PsychologyBase.ActivateKinsey())
                {
                    num *= realRecipient.sexuality.AdjustedRomanticDrive;
                }
            }
            num *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
            return Mathf.Clamp01(num);
        }

        [DetourMethod(typeof(InteractionWorker_MarriageProposal), "Interacted")]
        internal static void _Interacted(this InteractionWorker_MarriageProposal _this, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            float num = _this.AcceptanceChance(initiator, recipient);
            bool flag = Rand.Value < num;
            bool brokeUp = false;
            if (flag)
            {
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, initiator);
                foreach (ThoughtDef d in (from tgt in initiator.needs.mood.thoughts.memories.Memories
                                          where tgt.def.defName.Contains("RejectedMyProposal")
                                          select tgt.def))
                {
                    initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(d, recipient);
                }
                foreach (ThoughtDef d in (from tgt in recipient.needs.mood.thoughts.memories.Memories
                                          where tgt.def.defName.Contains("RejectedMyProposal")
                                          select tgt.def))
                {
                    recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(d, initiator);
                }
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, initiator);
                extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalAccepted);
            }
            else
            {
                extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejected);
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
                    if(rejectedProposalMoodStage.baseMoodEffect > 0)
                    {
                        realInitiator.needs.mood.thoughts.memories.TryGainMemoryThought(rejectedProposalMoodDef, realRecipient);
                    }
                    realInitiator.needs.mood.thoughts.memories.TryGainMemoryThought(rejectedProposalDef, realRecipient);
                }
                else
                {
                    initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RejectedMyProposal, recipient);
                }
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.IRejectedTheirProposal, initiator);
                if (realRecipient != null && !recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) && Rand.Value > 2f*realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic))
                {
                    recipient.interactions.TryInteractWith(initiator, DefDatabase<InteractionDef>.GetNamed("Breakup"));
                }
                else if(realRecipient == null && !recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) && Rand.Value < 0.4f)
                {
                    initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                    initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
                    brokeUp = true;
                    extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp);
                }
            }
            if (initiator.IsColonist || recipient.IsColonist)
            {
                var SendLetter = typeof(InteractionWorker_MarriageProposal).GetMethod("SendLetter", BindingFlags.Instance | BindingFlags.NonPublic);
                SendLetter.Invoke(_this, new object[] { initiator, recipient, flag, brokeUp });
            }
        }

        [DetourMethod(typeof(InteractionWorker_MarriageProposal), "RandomSelectionWeight")]
        internal static float _RandomSelectionWeight(this InteractionWorker_MarriageProposal _this, Pawn initiator, Pawn recipient)
        {
            DirectPawnRelation directRelation = initiator.relations.GetDirectRelation(PawnRelationDefOf.Lover, recipient);
            if (directRelation == null)
            {
                return 0f;
            }
            Pawn spouse = recipient.GetSpouse();
            Pawn spouse2 = initiator.GetSpouse();
            if ((spouse != null && !spouse.Dead) || (spouse2 != null && !spouse2.Dead))
            {
                return 0f;
            }
            float num = 0.4f;
            int ticksGame = Find.TickManager.TicksGame;
            float value = (float)(ticksGame - directRelation.startTicks) / GenDate.TicksPerDay;
            num *= Mathf.InverseLerp(0f, 60f, value);
            num *= Mathf.InverseLerp(0f, 60f, (float)initiator.relations.OpinionOf(recipient));
            if (recipient.relations.OpinionOf(initiator) < 0)
            {
                num *= 0.3f;
            }
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realInitiator != null)
            {
                num *= 0.1f + realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) + realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
                if(PsychologyBase.ActivateKinsey())
                {
                    num *= realInitiator.sexuality.AdjustedRomanticDrive;
                }
            }
            else if(initiator.gender == Gender.Female)
            {
                num *= 0.2f;
            }
            return num;
        }
    }
}
