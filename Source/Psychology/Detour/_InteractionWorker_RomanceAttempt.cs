using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    // Token: 0x02000328 RID: 808
    internal static class _InteractionWorker_RomanceAttempt
    {
        // Token: 0x06000E44 RID: 3652 RVA: 0x00049158 File Offset: 0x00047358
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt),"BreakLoverAndFianceRelations")]
        internal static void _BreakLoverAndFianceRelations(this InteractionWorker_RomanceAttempt _this, Pawn pawn, out List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            while (true)
            {
                Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                if (firstDirectRelationPawn != null && (!firstDirectRelationPawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) || !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)))
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
                    oldLoversAndFiances.Add(firstDirectRelationPawn);
                }
                else
                {
                    Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    if (firstDirectRelationPawn2 == null)
                    {
                        break;
                    }
                    else if (!firstDirectRelationPawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) || !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                    {
                        pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
                        oldLoversAndFiances.Add(firstDirectRelationPawn2);
                    }
                }
            }
        }

        // Token: 0x06000C7E RID: 3198 RVA: 0x0003DCB8 File Offset: 0x0003BEB8
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
        internal static float _RandomSelectionWeight(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            //Lovers won't romance each other
            if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                return 0f;
            }
            //Codependents won't romance anyone if they are in a relationship
            if (LovePartnerRelationUtility.HasAnyLovePartner(initiator) && initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                return 0f;
            }
            //No one will romance someone they find less than 25% attractive
            float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
            if (num < 0.25f)
            {
                return 0f;
            }
            //No one will romance someone they have less than +5 opinion of
            int num2 = initiator.relations.OpinionOf(recipient);
            if (num2 < 5)
            {
                return 0f;
            }
            //Only lechers will romance someone that has less than +5 opinion of them
            if (recipient.relations.OpinionOf(initiator) < 5 && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                return 0f;
            }
            //A pawn with +50 or more opinion of their lover will not hit on other pawns unless they are lecherous or polygamous (and their lover is also polygamous).
            float num3 = 1f;
            Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
            if (pawn != null && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher) && (!initiator.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)))
            {
                float value = (float)initiator.relations.OpinionOf(pawn);
                num3 = Mathf.InverseLerp(50f, -50f, value);
            }
            //Straight women are 15% as likely to romance anyone.
            float num4 = (!initiator.story.traits.HasTrait(TraitDefOf.Gay)) ? ((initiator.gender != Gender.Female) ? 1f : 0.15f) : 1f;
            float num5 = Mathf.InverseLerp(0.25f, 1f, num);
            float num6 = Mathf.InverseLerp(5f, 100f, (float)num2);
            //People who have hit on someone in the past and been rejected because of their sexuality will rarely attempt to hit on them again.
            float num7 = (realInitiator != null && realInitiator.sexuality.IncompatibleSexualityKnown(recipient)) ? 0.05f : 1f;
            //Only lechers will try to romance someone in a stable relationship.
            float num8 = 1f;
            Pawn pawn2 = LovePartnerRelationUtility.ExistingMostLikedLovePartner(recipient, false);
            if (pawn2 != null && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                int value = recipient.relations.OpinionOf(pawn2);
                num8 = Mathf.InverseLerp(5f, -100f, (float)value);
            }
            return 1.15f * num3 * num4 * num5 * num6 * num3 * num7 * num8;
        }

        // Token: 0x06000C80 RID: 3200 RVA: 0x0003DF08 File Offset: 0x0003C108
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt), "Interacted")]
        internal static void _Interacted(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if(PsychologyBase.ActivateKinsey())
            {
                PsychologyPawn realInitiator = initiator as PsychologyPawn;
                PsychologyPawn realRecipient = recipient as PsychologyPawn;
                if(realInitiator != null && realRecipient != null)
                {
                    realInitiator.sexuality.LearnSexuality(realRecipient);
                }
            }
            if (Rand.Value < _SuccessChance(_this, initiator, recipient))
            {
                List<Pawn> list;
                List<Pawn> list2;
                _this._BreakLoverAndFianceRelations(initiator, out list);
                _this._BreakLoverAndFianceRelations(recipient, out list2);
                for (int i = 0; i < list.Count; i++)
                {
                    _TryAddCheaterThought(_this, list[i], initiator);
                }
                for (int j = 0; j < list2.Count; j++)
                {
                    _TryAddCheaterThought(_this, list2[j], recipient);
                }
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
                TaleRecorder.RecordTale(TaleDefOf.BecameLover, new object[]
                {
                    initiator,
                    recipient
                });
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, initiator);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
                if (initiator.IsColonist || recipient.IsColonist)
                {
                    var _SendNewLoversLetter = typeof(InteractionWorker_RomanceAttempt).GetMethod("SendNewLoversLetter", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (_SendNewLoversLetter != null)
                        _SendNewLoversLetter.Invoke(_this, new object[] { initiator, recipient, list, list2 });
                    else
                        Log.ErrorOnce("Unable to reflect InteractionWorker_RomanceAttempt.SendNewLoversLetter!", 305432421);
                }
                extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
                LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
            }
            else
            {
                if(!initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
                    initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.RebuffedMyRomanceAttemptLecher, recipient);
                else
                    initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
                if (recipient.relations.OpinionOf(initiator) <= 0)
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
                }
                extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptRejected);
            }
        }

        // Token: 0x06000C7F RID: 3199 RVA: 0x0003DDB0 File Offset: 0x0003BFB0
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
        internal static float _SuccessChance(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient)
        {
            float num = 0.6f;
            num *= recipient.relations.SecondaryRomanceChanceFactor(initiator);
            num *= Mathf.InverseLerp(5f, 100f, (float)recipient.relations.OpinionOf(initiator));
            float num2 = 1f;
            if (!recipient.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
            {
                Pawn pawn = null;
                if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                    num2 = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.6f;
                }
                else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    num2 = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.1f;
                }
                else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead)
                {
                    pawn = recipient.GetSpouse();
                    num2 = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.3f;
                }
                if (pawn != null)
                {
                    num2 *= Mathf.InverseLerp(100f, 0f, (float)recipient.relations.OpinionOf(pawn));
                    num2 *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
                }
            }
            if(recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                num2 = 1.916f;
            }
            num *= num2;
            return Mathf.Clamp01(num);
        }

        // Token: 0x06000C82 RID: 3202 RVA: 0x0003E16C File Offset: 0x0003C36C
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt), "TryAddCheaterThought")]
        internal static void _TryAddCheaterThought(this InteractionWorker_RomanceAttempt _this, Pawn pawn, Pawn cheater)
        {
            if (pawn.Dead)
            {
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.CheatedOnMe, cheater);
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CheatedOnMeCodependent, cheater);
        }

        [DetourFallback(new string[] { "_BreakLoverAndFianceRelations", "_RandomSelectionWeight", "_SuccessChance", "_Interacted", "_TryAddCheaterThought" })]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursSexual = false;
        }
    }
}
