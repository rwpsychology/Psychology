using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using UnityEngine;

namespace Psychology.Harmony
{
    /*[HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "DoRecruit")]
    public static class InteractionWorker_RecruitAttempt_DoRecruitPatch
    {
        [HarmonyPostfix]
        public static void AddCapturedThoughts(Pawn recruiter, Pawn recruitee)
        {
            if (recruitee.RaceProps.Humanlike)
            {
                recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
                recruitee.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.RapportBuilt);
                IEnumerable<Pawn> allFactionPawns = Find.Maps.SelectMany(m => from p in m.mapPawns.FreeColonistsSpawned
                                                                       where p != recruitee
                                                                       select p);
                foreach (Pawn pawn in allFactionPawns)
                {
                    recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CapturedMe, pawn);
                }
                if(PsycheHelper.PsychologyEnabled(recruitee))
                {
                    PsycheHelper.Comp(recruitee).Recruiting = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "Interacted")]
    public static class InteractionWorker_RecruitAttempt_RecruitingHook
    {
        [HarmonyPrefix]
        public static bool ReplaceVanillaRecruiting(this InteractionWorker_RecruitAttempt __instance, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {

            if (Traverse.Create<Pawn_MindState>().Method("CheckStartMentalStateBecauseRecruitAttempted").GetValue<bool>((new object[] { recipient, initiator })))
            {
                return false;
            }
            bool flag = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment && recipient.RaceProps.Humanlike;
            float recruitChance = 1f;
            if (flag || DebugSettings.instantRecruit)
            {
                InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, recruitChance, true);
                extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
                if (flag)
                {
                    initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
                }
            }
            else
            {
                if(recipient.NonHumanlikeOrWildMan() || !(recipient is PsychologyPawn))
                {
                    recruitChance *= ((!recipient.NonHumanlikeOrWildMan()) ? initiator.GetStatValue(StatDefOf.RecruitPrisonerChance, true) : initiator.GetStatValue(StatDefOf.TameAnimalChance, true));
                    float num2;
                    if (recipient.IsWildMan())
                    {
                        num2 = 0.25f;
                    }
                    else if (recipient.RaceProps.Humanlike)
                    {
                        num2 = 1f - recipient.RecruitDifficulty(initiator.Faction, true);
                    }
                    else
                    {
                        num2 = Traverse.Create<InteractionWorker_RecruitAttempt>().Field("RecruitChanceFactorCurve_Wildness").GetValue<SimpleCurve>().Evaluate(recipient.RaceProps.wildness);
                    }
                    recruitChance *= num2;
                    if (!recipient.NonHumanlikeOrWildMan())
                    {
                        float x = (float)recipient.relations.OpinionOf(initiator);
                        recruitChance *= Traverse.Create<InteractionWorker_RecruitAttempt>().Field("RecruitChanceFactorCurve_Opinion").GetValue<SimpleCurve>().Evaluate(x);
                        if (recipient.needs.mood != null)
                        {
                            float curLevel = recipient.needs.mood.CurLevel;
                            recruitChance *= Traverse.Create<InteractionWorker_RecruitAttempt>().Field("RecruitChanceFactorCurve_Mood").GetValue<SimpleCurve>().Evaluate(curLevel);
                        }
                    }
                    if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
                    {
                        recruitChance *= 4f;
                    }
                    recruitChance = Mathf.Clamp(recruitChance, 0.005f, 1f);
                    if (Rand.Chance(recruitChance))
                    {
                        InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, recruitChance, true);
                        extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
                        if (flag)
                        {
                            initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.InspiredRecruitment);
                        }
                    }
                    else
                    {
                        string text;
                        if (recipient.NonHumanlikeOrWildMan())
                        {
                            text = "TextMote_TameFail".Translate(new object[]
                            {
                                recruitChance.ToStringPercent()
                            });
                        }
                        else
                        {
                            text = "TextMote_RecruitFail".Translate(new object[]
                            {
                                recruitChance.ToStringPercent()
                            });
                        }
                        MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
                        extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
                    }
                }
                else
                {
                    PsychologyPawn realPawn = recipient as PsychologyPawn;
                    PsychologyPawn realRecruiter = initiator as PsychologyPawn;
                    RecruitingUtility.PrepareForRecruiting(realPawn);
                    LordJob_WardenTour tour = new LordJob_WardenTour(realRecruiter, realPawn);
                    List<Pawn> pawns = new List<Pawn>();
                    pawns.Add(realPawn);
                    pawns.Add(realRecruiter);
                    LordMaker.MakeNewLord(realRecruiter.Faction, tour, realPawn.Map, pawns);
                }
            }
            return false;
        }
    }*/
}
