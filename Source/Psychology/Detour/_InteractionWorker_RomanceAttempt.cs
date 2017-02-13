using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _InteractionWorker_RomanceAttempt
    {
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
                    else if (!firstDirectRelationPawn2.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) || !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                    {
                        pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
                        oldLoversAndFiances.Add(firstDirectRelationPawn2);
                    }
                }
            }
        }

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
            float attractiveness = initiator.relations.SecondaryRomanceChanceFactor(recipient);
            if (attractiveness < 0.25f)
            {
                return 0f;
            }
            //No one will romance someone they have less than +5 opinion of
            int opinion = initiator.relations.OpinionOf(recipient);
            if (opinion < 5)
            {
                return 0f;
            }
            //Only lechers will romance someone that has less than +5 opinion of them
            if (recipient.relations.OpinionOf(initiator) < 5 && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                return 0f;
            }
            float romanceChance = 1.15f;
            if(realInitiator == null)
            {
                //Straight women are 15% as likely to romance anyone.
                romanceChance = (!initiator.story.traits.HasTrait(TraitDefOf.Gay)) ? ((initiator.gender != Gender.Female) ? romanceChance : romanceChance*0.15f) : romanceChance;
            }
            else
            {
                //A pawn's likelihood to romance is based on how Aggressive and Romantic they are.
                romanceChance = 0.2f + realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) + (1f - realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
            }
            //A pawn with +50 or more opinion of their lover will not hit on other pawns unless they are lecherous or polygamous (and their lover is also polygamous).
            float existingLovePartnerFactor = 1f;
            Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
            if (pawn != null && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher) && (!initiator.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)))
            {
                float value = (float)initiator.relations.OpinionOf(pawn);
                existingLovePartnerFactor = Mathf.InverseLerp(50f, -50f, value);
            }
            float attractivenessFactor = Mathf.InverseLerp(0.25f, 1f, attractiveness);
            float opinionFactor = Mathf.InverseLerp(5f, 100f, (float)opinion);
            //People who have hit on someone in the past and been rejected because of their sexuality will rarely attempt to hit on them again.
            float knownSexualityFactor = (realInitiator != null && realInitiator.sexuality.IncompatibleSexualityKnown(recipient) && !realInitiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher)) ? 0.05f : 1f;
            //Only lechers will try to romance someone in a stable relationship.
            float recipientLovePartnerFactor = 1f;
            Pawn pawn2 = LovePartnerRelationUtility.ExistingMostLikedLovePartner(recipient, false);
            if (pawn2 != null && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                int value = recipient.relations.OpinionOf(pawn2);
                recipientLovePartnerFactor = Mathf.InverseLerp(5f, -100f, (float)value);
            }
            return romanceChance * existingLovePartnerFactor * attractivenessFactor * opinionFactor * knownSexualityFactor * recipientLovePartnerFactor;
        }
        
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
                foreach (PawnRelationDef d in (from rel in initiator.relations.DirectRelations
                                               where rel.def.defName.Contains("ExLover")
                                               select rel.def))
                {
                    initiator.relations.TryRemoveDirectRelation(d, recipient);
                }
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
                TaleRecorder.RecordTale(TaleDefOf.BecameLover, new object[]
                {
                    initiator,
                    recipient
                });
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, recipient);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, initiator);
                foreach (ThoughtDef d in (from tgt in initiator.needs.mood.thoughts.memories.Memories
                                               where tgt.def.defName.Contains("BrokeUpWithMe")
                                               select tgt.def))
                {
                    initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(d, recipient);
                }
                foreach (ThoughtDef d in (from tgt in recipient.needs.mood.thoughts.memories.Memories
                                          where tgt.def.defName.Contains("BrokeUpWithMe")
                                          select tgt.def))
                {
                    recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(d, initiator);
                }
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
                if(initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
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
        
        [DetourMethod(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
        internal static float _SuccessChance(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient)
        {

            float successChance = 0.6f;
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            if(realRecipient != null)
            {
                //The recipient is less likely to accept the more romantic they are, which means they will need to like the person more.
                successChance = 0.25f + (1f - realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
            }
            successChance *= recipient.relations.SecondaryRomanceChanceFactor(initiator);
            successChance *= 2f*Mathf.InverseLerp(5f, 100f, (float)recipient.relations.OpinionOf(initiator));
            float existingLovePartnerFactor = 1f;
            if (!recipient.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
            {
                Pawn pawn = null;
                if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.6f;
                }
                else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.1f;
                }
                else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead)
                {
                    pawn = recipient.GetSpouse();
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0f : 0.3f;
                }
                if (pawn != null)
                {
                    existingLovePartnerFactor *= Mathf.InverseLerp(100f, 0f, (float)recipient.relations.OpinionOf(pawn));
                    existingLovePartnerFactor *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
                }
            }
            if(recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                existingLovePartnerFactor = 1.916f;
            }
            successChance *= existingLovePartnerFactor;
            return Mathf.Clamp01(successChance);
        }
        
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
