using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Psychology
{
    class LordJob_Joinable_Election: LordJob_VoluntarilyJoinable
    {
        public LordJob_Joinable_Election()
        { }
        
        public LordJob_Joinable_Election(IntVec3 spot)
        {
            this.spot = spot;
        }

        public LordJob_Joinable_Election(IntVec3 spot, List<Candidate> candidates, string baseName)
        {
            this.spot = spot;
            this.candidates = candidates;
            this.baseName = baseName;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Election lordToil_Election = new LordToil_Election(this.spot);
            stateGraph.AddToil(lordToil_Election);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_Election, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            transition.AddTrigger(new Trigger_TickCondition(() => this.candidates.Count == 0));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddPreAction(new TransitionAction_Message("MessageElectionCalledOff".Translate(this.baseName), MessageSound.Negative, new TargetInfo(this.spot, this.Map, false)));
            stateGraph.AddTransition(transition);
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(GenDate.TicksPerHour * 4, GenDate.TicksPerHour * 8));
            Transition transition2 = new Transition(lordToil_Election, lordToil_End);
            transition2.AddTrigger(this.timeoutTrigger);
            transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                this.Finished();
            }));
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.spot, "spot", default(IntVec3));
            Scribe_Values.Look(ref this.baseName, "settlementName", "a settlement");
            Scribe_Collections.Look(ref this.candidates, "candidates", LookMode.Deep, new object[0]);
            Scribe_Collections.Look(ref this.voters, "voters", LookMode.Value, new object[0]);
            Scribe_Collections.Look(ref this.votes, "votes", LookMode.Value, new object[0]);
        }
        
        private void Finished()
        {
            List<Pair<PsychologyPawn, int>> voteTally = new List<Pair<PsychologyPawn, int>>();
            foreach (Candidate candidate in this.candidates)
            {
                IEnumerable<string> votesForMe = (from v in this.votes
                                           where v == candidate.pawn.LabelShort
                                           select v);
                voteTally.Add(new Pair<PsychologyPawn, int>(candidate.pawn, votesForMe.Count()));
            }
            //If there ends up being a tie, we'll just assume the least competitive candidates drop out.
            //The chances of there being a tie after that are exceedingly slim, but the result will be essentially random.
            IEnumerable<Pair<PsychologyPawn, int>> orderedTally = (from v in voteTally
                                                                   orderby v.First.psyche.GetPersonalityRating(PersonalityNodeDefOf.Competitive) descending
                                                                   orderby v.Second descending
                                                                   select v);
            if (Prefs.DevMode && Prefs.LogVerbose)
            {
                foreach(Pair<PsychologyPawn, int> t in orderedTally)
                {
                    Log.Message("[Psychology] Votes for " + t.First + ": " + t.Second);
                }
            }
            Pair<PsychologyPawn, int> winningCandidate = orderedTally.First();
            if (orderedTally.Count() > 1 && orderedTally.First().Second == orderedTally.ElementAt(1).Second)
            {
                Find.LetterStack.ReceiveLetter("LetterLabelTieSettled".Translate(winningCandidate.First.LabelShort), "LetterTieSettled".Translate(winningCandidate.First.LabelShort).AdjustedFor(winningCandidate.First), LetterDefOf.BadNonUrgent, winningCandidate.First);
            }
            StringBuilder issuesString = new StringBuilder();
            for (int i = 0; i < candidates.Find(c => c.pawn == winningCandidate.First).nodes.Count; i++)
            {
                issuesString.AppendFormat("{0}) {1}{2}", i + 1, winningCandidate.First.psyche.GetPersonalityNodeOfDef(candidates.Find(c => c.pawn == winningCandidate.First).nodes[i]).PlatformIssue, (i != candidates.Find(c => c.pawn == winningCandidate.First).nodes.Count - 1 ? "\n" : ""));
            }
            Hediff mayor = HediffMaker.MakeHediff(HediffDefOfPsychology.Mayor, winningCandidate.First);
            winningCandidate.First.health.AddHediff(mayor);
            winningCandidate.First.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.WonElection);
            Find.LetterStack.ReceiveLetter("LetterLabelElectionWon".Translate(winningCandidate.First.LabelShort), "LetterElectionWon".Translate(winningCandidate.First.LabelShort, this.baseName, winningCandidate.Second, issuesString.ToString()).AdjustedFor(winningCandidate.First), LetterDefOf.Good, winningCandidate.First);
        }
        
        public override string GetReport()
        {
            return "LordReportAttendingElection".Translate();
        }
        
        private bool IsInvited(Pawn p)
        {
            return p.Faction == this.lord.faction;
        }
        
        private bool IsPartyAboutToEnd()
        {
            return this.timeoutTrigger.TicksLeft < 1200;
        }
        
        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(base.Map) || candidates.Count < 1;
        }
        
        private bool ShouldPawnKeepVoting(Pawn p)
        {
            if(!(p is PsychologyPawn))
            {
                return false;
            }
            PsychologyPawn realPawn = p as PsychologyPawn;
            bool matchingCandidates = (from c in candidates
                                       where c.pawn == realPawn
                                       select c).Count() > 0;
            if (voters.Contains(p.GetHashCode()) && !matchingCandidates)
            {
                return false;
            }
            bool notApathetic = realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Passionate) > (0.6f / candidates.Count);
            return GatheringsUtility.ShouldGuestKeepAttendingGathering(p) && (notApathetic || matchingCandidates);
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (!this.IsInvited(p))
            {
                return 0f;
            }
            if (!this.ShouldPawnKeepVoting(p))
            {
                return 0f;
            }
            if (!this.lord.ownedPawns.Contains(p) && this.IsPartyAboutToEnd())
            {
                return 0f;
            }
            return 30f;
        }
        
        private IntVec3 spot;
        private string baseName;
        private Trigger_TicksPassed timeoutTrigger;
        public List<string> votes = new List<string>();
        public List<int> voters = new List<int>();
        public List<Candidate> candidates = new List<Candidate>();
    }
}
