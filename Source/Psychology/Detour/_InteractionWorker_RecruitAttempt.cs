using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;
using UnityEngine;

namespace Psychology.Detour
{
    internal static class _InteractionWorker_RecruitAttempt
    {
        // Token: 0x06000E3F RID: 3647 RVA: 0x00048A20 File Offset: 0x00046C20
        [DetourMethod(typeof(InteractionWorker_RecruitAttempt),"DoRecruit")]
        internal static void _DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
        {
            string text = recruitee.LabelIndefinite();
            if (recruitee.guest != null)
            {
                recruitee.guest.SetGuestStatus(null, false);
            }
            bool flag = recruitee.Name != null;
            if (recruitee.Faction != recruiter.Faction)
            {
                recruitee.SetFaction(recruiter.Faction, recruiter);
            }
            if (recruitee.RaceProps.Humanlike)
            {
                if (useAudiovisualEffects)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelMessageRecruitSuccess".Translate(), "MessageRecruitSuccess".Translate(new object[]
                    {
                        recruiter,
                        recruitee,
                        recruitChance.ToStringPercent()
                    }), LetterType.Good, recruitee, null);
                }
                TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[]
                {
                    recruiter,
                    recruitee
                });
                recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
                recruitee.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RecruitedMe, recruiter);
                recruitee.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.RapportBuilt);
                List<Pawn> allFactionPawns = Find.Maps.SelectMany(m => from p in m.mapPawns.PawnsInFaction(recruiter.Faction)
                                                                  where p != recruitee && p.RaceProps.Humanlike
                                                                  select p).ToList<Pawn>();
                foreach (Pawn pawn in allFactionPawns)
                {
                    recruitee.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CapturedMe, pawn);
                }
            }
            else
            {
                if (useAudiovisualEffects)
                {
                    if (!flag)
                    {
                        Messages.Message("MessageTameAndNameSuccess".Translate(new object[]
                        {
                            recruiter.LabelShort,
                            text,
                            recruitChance.ToStringPercent(),
                            recruitee.Name.ToStringFull
                        }).AdjustedFor(recruitee), recruitee, MessageSound.Benefit);
                    }
                    else
                    {
                        Messages.Message("MessageTameSuccess".Translate(new object[]
                        {
                            recruiter.LabelShort,
                            text,
                            recruitChance.ToStringPercent()
                        }), recruitee, MessageSound.Benefit);
                    }
                    MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(new object[]
                    {
                        recruitChance.ToStringPercent()
                    }), 8f);
                }
                recruiter.records.Increment(RecordDefOf.AnimalsTamed);
                RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
                float num = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
                if (Rand.Value < num)
                {
                    TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, new object[]
                    {
                        recruiter,
                        recruitee
                    });
                }
            }
            if (recruitee.caller != null)
            {
                recruitee.caller.DoCall();
            }
        }
    }
}
