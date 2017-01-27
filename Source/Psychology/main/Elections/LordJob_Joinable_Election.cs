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
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 15000));
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
            Scribe_Values.LookValue(ref this.spot, "spot", default(IntVec3));
            Scribe_Values.LookValue(ref this.baseName, "settlementName", "a settlement");
            Scribe_Collections.LookList(ref this.candidates, "candidates", LookMode.Deep, new object[0]);
            Scribe_Collections.LookList(ref this.voters, "voters", LookMode.Reference, new object[0]);
            Scribe_Collections.LookList(ref this.votes, "votes", LookMode.Value, new object[0]);
        }
        
        private void Finished()
        {
            List<Pair<PsychologyPawn, int>> voteTally = new List<Pair<PsychologyPawn, int>>();
            foreach (Candidate candidate in this.candidates)
            {
                List<string> votesForMe = (from v in this.votes
                                           where v == candidate.pawn.LabelShort
                                           select v).ToList();
                voteTally.Add(new Pair<PsychologyPawn, int>(candidate.pawn, votesForMe.Count));
            }
            voteTally = voteTally.OrderByDescending(pair => pair.Second).ToList();
            voteTally.ForEach(t => Log.Message(t.First + ": "+ t.Second));
            Pair<PsychologyPawn, int> winningCandidate = voteTally[0];
            StringBuilder issuesString = new StringBuilder();
            for (int i = 0; i < candidates.Find(c => c.pawn == winningCandidate.First).nodes.Count; i++)
            {
                issuesString.AppendFormat("{0}) {1}{2}", i + 1, winningCandidate.First.psyche.GetPersonalityNodeOfDef(candidates.Find(c => c.pawn == winningCandidate.First).nodes[i]).PlatformIssue, (i != candidates.Find(c => c.pawn == winningCandidate.First).nodes.Count - 1 ? "\n" : ""));
            }
            Hediff mayor = HediffMaker.MakeHediff(HediffDefOfPsychology.Mayor, winningCandidate.First);
            winningCandidate.First.health.AddHediff(mayor);
            Find.LetterStack.ReceiveLetter("LetterLabelElectionWon".Translate(winningCandidate.First.LabelShort), "LetterElectionWon".Translate(winningCandidate.First.LabelShort, this.baseName, winningCandidate.Second, issuesString.ToString()).AdjustedFor(winningCandidate.First), LetterType.Good, winningCandidate.First);
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
            return !PartyUtility.AcceptableMapConditionsToContinueParty(base.Map) || candidates.Count < 1;
        }
        
        private bool ShouldPawnKeepVoting(Pawn p)
        {
            return GatheringsUtility.ShouldGuestKeepAttendingGathering(p) && (p is PsychologyPawn && ((((PsychologyPawn)p).psyche.GetPersonalityRating(PersonalityNodeDefOf.Passionate) > (0.6f/candidates.Count) && !voters.Contains(p)) || candidates.Find(c => c.pawn == (PsychologyPawn)p) != null));
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
            return 20f;
        }
        
        private IntVec3 spot;
        private string baseName;
        private Trigger_TicksPassed timeoutTrigger;
        public List<string> votes = new List<string>();
        public List<Pawn> voters = new List<Pawn>();
        public List<Candidate> candidates = new List<Candidate>();
    }
}
