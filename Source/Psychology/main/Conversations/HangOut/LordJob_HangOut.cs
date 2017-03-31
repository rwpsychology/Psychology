using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;

namespace Psychology
{
    class LordJob_HangOut : LordJob
    {
        public LordJob_HangOut()
        { }

        public LordJob_HangOut(Pawn initiator, Pawn recipient)
        {
            this.initiator = initiator;
            this.recipient = recipient;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_HangOut lordToil_HangOut = new LordToil_HangOut(new Pawn[] { initiator, recipient });
            stateGraph.AddToil(lordToil_HangOut);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_HangOut, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            transition.AddTrigger(new Trigger_TickCondition(() => this.initiator.health.summaryHealth.SummaryHealthPercent < 1f || this.recipient.health.summaryHealth.SummaryHealthPercent < 1f));
            transition.AddTrigger(new Trigger_TickCondition(() => this.initiator.Drafted || this.recipient.Drafted));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            stateGraph.AddTransition(transition);
            //Time of meeting is affected by the constituents' mood; meetings to complain can take longer than meetings to commend.
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(GenDate.TicksPerHour *3, GenDate.TicksPerHour*5));
            Transition transition2 = new Transition(lordToil_HangOut, lordToil_End);
            transition2.AddTrigger(this.timeoutTrigger);
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }
        
        public override void ExposeData()
        {
            Scribe_References.LookReference(ref this.initiator, "initiator");
            Scribe_References.LookReference(ref this.recipient, "recipient");
        }
        
        public override string GetReport()
        {
            return "LordReportHangingOut".Translate();
        }
        
        private bool IsPartyAboutToEnd()
        {
            return this.timeoutTrigger.TicksLeft < 1200;
        }
        
        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableMapConditionsToContinueParty(base.Map) || this.initiator.GetTimeAssignment() == TimeAssignmentDefOf.Work || this.recipient.GetTimeAssignment() == TimeAssignmentDefOf.Work || this.initiator.needs.rest.CurLevel < 0.3f || this.recipient.needs.rest.CurLevel < 0.3f;
        }
        
        private Trigger_TicksPassed timeoutTrigger;
        public Pawn initiator;
        public Pawn recipient;
    }
}
