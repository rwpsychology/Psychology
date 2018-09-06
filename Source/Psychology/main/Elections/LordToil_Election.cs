using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class LordToil_Election : LordToil
    {
        public LordToil_Election(IntVec3 spot)
        {
            this.spot = spot;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfPsychology.Vote, this.spot, -1f);
            }
        }
        
        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            return DutyDefOfPsychology.Vote.hook;
        }

        [LogPerformance]
        public override void Notify_ReachedDutyLocation(Pawn voter)
        {
            LordJob_Joinable_Election election = voter.GetLord().LordJob as LordJob_Joinable_Election;
            if(election != null && PsycheHelper.PsychologyEnabled(voter) && !election.voters.Contains(voter.GetHashCode()))
            {
                election.voters.Add(voter.GetHashCode());
                if(election.candidates.Find(c => c.pawn == voter) == null)
                {
                    List<Pair<Pawn, float>> possibleVotes = new List<Pair<Pawn, float>>();
                    foreach (Candidate candidate in election.candidates)
                    {
                        float issueWeighting = 0f;
                        candidate.nodes.ForEach(p => issueWeighting += (4f * Mathf.Pow(1f - Mathf.Abs(PsycheHelper.Comp(candidate.pawn).Psyche.GetPersonalityRating(p) - PsycheHelper.Comp(voter).Psyche.GetPersonalityRating(p)), 5f)) * Mathf.Pow(2.5f, p.controversiality));
                        possibleVotes.Add(new Pair<Pawn, float>(candidate.pawn, issueWeighting+voter.relations.OpinionOf(candidate.pawn)));
                    }
                    IEnumerable<Pair<Pawn, float>> orderedPossibleVotes = (from v in possibleVotes
                                                                           orderby v.Second descending
                                                                           select v);
                    if (Prefs.DevMode && Prefs.LogVerbose)
                    {
                        StringBuilder voteString = new StringBuilder("Psychology :: Vote weights for " + voter.LabelShort + ": ");
                        foreach (Pair<Pawn, float> v in orderedPossibleVotes)
                        {
                            voteString.Append(v.First.LabelShort + " " + v.Second + " ");
                        }
                        Log.Message(voteString.ToString());
                    }
                    election.votes.Add(orderedPossibleVotes.First().First.LabelShort);
                }
                else
                {
                    election.votes.Add(voter.LabelShort);
                }
            }
        }

        private IntVec3 spot;
    }
}
