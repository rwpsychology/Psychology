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
    internal static class _InteractionWorker
    {
        [DetourMethod(typeof(InteractionWorker), "Interacted")]
        internal static void _Interacted(this InteractionWorker _this, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if (_this.GetType() == typeof(InteractionWorker_DeepTalk))
            {
                if (initiator.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Homesickness) > 0 && Rand.Value < (0.1f + initiator.GetStatValue(StatDefOf.SocialImpact)))
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Homesickness);
                }
            }
            List<Pawn> cliquers = (from p in initiator.Map.mapPawns.AllPawnsSpawned
                                   where p.RaceProps.Humanlike && !p.Dead && p.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0
                                   select p).ToList(); //try to save FPS with just one sweep
            if(cliquers.Count > 0 && initiator.RaceProps.Humanlike && recipient.RaceProps.Humanlike)
            {
                List<Pawn> initiatorFriends = (from p in initiator.Map.mapPawns.AllPawnsSpawned
                                               where p.RaceProps.Humanlike && !p.Dead && initiator.relations.OpinionOf(p) > 20 && recipient != p
                                               select p).ToList();
                List<Pawn> recipientFriends = (from p in initiator.Map.mapPawns.AllPawnsSpawned
                                               where p.RaceProps.Humanlike && !p.Dead && recipient.relations.OpinionOf(p) > 20 && initiator != p
                                               select p).ToList();
                List<Pawn> cliqueLinks = new List<Pawn>();
                bool checkInitiator = true;
                bool clique = (initiator.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && initiator.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == recipient);
                if (!clique)
                {
                    //Any of the initiator's friends lead the clique against the recipient
                    cliqueLinks.AddRange((from f in initiatorFriends
                                          where f.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && f.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == recipient && initiator.relations.OpinionOf(f) > initiator.relations.OpinionOf(recipient)
                                          select f));
                    if (cliqueLinks.Count == 0)
                    {
                        checkInitiator = false;
                        //Any of the recipient's friends lead the clique against the initiator
                        cliqueLinks.AddRange((from f in recipientFriends
                                              where f.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && f.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == initiator && recipient.relations.OpinionOf(f) > recipient.relations.OpinionOf(initiator)
                                              select f));
                    }
                    if (cliqueLinks.Count == 0)
                    {
                        //The initiator leads a clique against any of the recipient's friends
                        cliqueLinks.AddRange((from f in recipientFriends
                                              where initiator.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && initiator.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == f && recipient.relations.OpinionOf(f) > recipient.relations.OpinionOf(initiator)
                                              select f));
                    }
                    if (cliqueLinks.Count == 0)
                    {
                        checkInitiator = true;
                        //The recipient leads a clique against any of the initiator's friends
                        cliqueLinks.AddRange((from f in initiatorFriends
                                              where recipient.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && recipient.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == f && initiator.relations.OpinionOf(f) > initiator.relations.OpinionOf(recipient)
                                              select f));
                    }
                    if (cliqueLinks.Count == 0)
                    {
                        //Any of the initiator's friends leads a clique against any of the recipient's friends
                        //This should be the same in either direction since they both have the thought against each other
                        cliqueLinks.AddRange(initiatorFriends.SelectMany(x => (from f in recipientFriends
                                                                               where x.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0 && x.needs.mood.thoughts.memories.OldestMemoryThoughtOfDef(ThoughtDefOfPsychology.Clique).otherPawn == f && !(initiatorFriends.Contains(f) && recipientFriends.Contains(x))
                                                                               select f)));
                    }
                }
                if (clique || cliqueLinks.Count > 0)
                {
                    if (Rand.Value <= 0.05f && lastFightTick <= Find.TickManager.TicksGame - 5000)
                    {
                        if (!checkInitiator && Rand.Value <= Mathf.InverseLerp(50f, -100f, recipient.relations.OpinionOf(initiator)))
                        {
                            if (!_ThoughtUtility.FindThoughtInMemories(recipient, ThoughtDefOfPsychology.Clique, initiator))
                            {
                                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CliqueFollower, initiator);
                            }
                            recipient.interactions.StartSocialFight(initiator);
                        }
                        else if (Rand.Value <= Mathf.InverseLerp(50f, -100f, initiator.relations.OpinionOf(recipient)))
                        {
                            if (!_ThoughtUtility.FindThoughtInMemories(initiator, ThoughtDefOfPsychology.Clique, recipient))
                            {
                                initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CliqueFollower, recipient);
                            }
                            initiator.interactions.StartSocialFight(recipient);
                        }
                        lastFightTick = Find.TickManager.TicksGame;
                    }
                }
            }
        }

        private static int lastFightTick = -9999;
    }
}
