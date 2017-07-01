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
    class LordJob_VisitMayor : LordJob
    {
        public LordJob_VisitMayor()
        { }
        
        public LordJob_VisitMayor(IntVec3 spot)
        {
            this.spot = spot;
        }

        public LordJob_VisitMayor(IntVec3 spot, Pawn constituent, Pawn mayor, bool complaint)
        {
            this.spot = spot;
            this.constituent = constituent;
            this.mayor = mayor;
            this.complaint = complaint;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Meeting lordToil_Meeting = new LordToil_Meeting(this.spot);
            stateGraph.AddToil(lordToil_Meeting);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_Meeting, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            transition.AddTrigger(new Trigger_TickCondition(() => this.mayor.health.summaryHealth.SummaryHealthPercent < 1f || this.constituent.health.summaryHealth.SummaryHealthPercent < 1f));
            transition.AddTrigger(new Trigger_TickCondition(() => this.mayor.Drafted || this.constituent.Drafted));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            stateGraph.AddTransition(transition);
            //Time of meeting is affected by the constituents' mood; meetings to complain can take longer than meetings to commend.
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(GenDate.TicksPerHour, Mathf.RoundToInt(GenDate.TicksPerHour / Mathf.Lerp(0.2f, 1f, constituent.needs.mood.CurLevel))));
            Transition transition2 = new Transition(lordToil_Meeting, lordToil_End);
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
            Scribe_References.Look(ref this.constituent, "constituent");
            Scribe_References.Look(ref this.mayor, "mayor");
            Scribe_Values.Look(ref this.complaint, "complaining");
        }
        
        private void Finished()
        {
            PsychologyPawn realMayor = mayor as PsychologyPawn;
            PsychologyPawn realConstituent = constituent as PsychologyPawn;
            if(realMayor != null & realConstituent != null)
            {
                if (this.ticksInSameRoom > 0)
                {
                    if (this.complaint)
                    {
                        ThoughtDef complaintDef = new ThoughtDef();
                        complaintDef.label = "MayorComplaint";
                        complaintDef.durationDays = 1f + 4f * this.mayor.GetStatValue(StatDefOf.SocialImpact);
                        //Constituent thought duration affected by mayor's Social stat
                        complaintDef.thoughtClass = typeof(Thought_MemoryDynamic);
                        complaintDef.stackedEffectMultiplier = 1f;
                        ThoughtStage complaintStage = new ThoughtStage();
                        float complaintMood = 12f * (realMayor.psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic) - 0.33f);
                        //Base complaint mood determined by mayor's Empathetic trait
                        complaintMood *= this.ticksInSameRoom / GenDate.TicksPerHour;
                        //Length of meeting also affects mood
                        complaintMood *= (complaintMood < 0f ? Mathf.Lerp(1.25f, 0.75f, realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                        //Negative meeting thoughts (unempathetic mayors) mitigated by mayor's politeness
                        complaintMood += (BeautyUtility.AverageBeautyPerceptible(this.constituent.Position, this.constituent.Map) / 10f);
                        //Beauty of the room has a net positive effect on the thought
                        complaintMood *= 0.75f + (realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental)/2f);
                        //Constituent's Judgmental trait changes how much the thought affects them
                        complaintStage.label = "ComplaintLabel".Translate();
                        complaintStage.description = "ComplaintDesc".Translate();
                        complaintStage.baseMoodEffect = Mathf.RoundToInt(complaintMood);
                        complaintDef.defName = this.constituent.GetHashCode() + "MayorComplaint" + complaintStage.baseMoodEffect;
                        complaintDef.stages.Add(complaintStage);
                        this.constituent.needs.mood.thoughts.memories.TryGainMemory(complaintDef, this.mayor);
                    }
                    ThoughtDef visitDef = new ThoughtDef();
                    visitDef.label = "MayorVisited";
                    visitDef.durationDays = 0.75f + 2f * (1f - realMayor.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent));
                    //Mayor thought duration affected by mayor's Independent trait
                    visitDef.thoughtClass = typeof(Thought_MemoryDynamic);
                    visitDef.stackedEffectMultiplier = 1f;
                    ThoughtStage stage = new ThoughtStage();
                    float mood = 5f * (complaint ? -1f - (1f - this.constituent.needs.mood.CurLevel) : 0.25f + Mathf.Max(0f, 0.25f - this.constituent.needs.mood.CurLevel));
                    //Base visit mood determined by the mood level of the constituent
                    mood *= this.ticksInSameRoom / GenDate.TicksPerHour;
                    //Length of meeting also affects mood
                    mood *= (mood < 0f ? Mathf.Lerp(1.25f, 0.75f, realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                    //Negative meeting thoughts (unhappy constituents) mitigated by constituent's politeness
                    mood *= 0.5f + (1f - realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack));
                    //Mayor's Laid-Back trait strongly impacts how much the thought affects them
                    stage.label = "VisitLabel".Translate();
                    stage.description = "VisitDesc".Translate();
                    stage.baseMoodEffect = Mathf.RoundToInt(mood);
                    visitDef.defName = this.mayor.GetHashCode() + "MayorVisited" + stage.baseMoodEffect;
                    visitDef.stages.Add(stage);
                    this.mayor.needs.mood.thoughts.memories.TryGainMemory(visitDef, this.constituent);
                    InteractionDef endConversation = new InteractionDef();
                    endConversation.defName = "EndConversation";
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    RulePack goodbyeTextInit = new RulePack();
                    List<string> text = new List<string>(1);
                    if (complaint)
                    {
                        text.Add("logentry->"+"Complained".Translate());
                    }
                    else
                    {
                        text.Add("logentry->"+"Supported".Translate());
                    }
                    RuleStrings.SetValue(goodbyeTextInit, text);
                    RulePack goodbyeTextRecip = new RulePack();
                    List<String> text2 = new List<string>(1);
                    text2.Add("logentry->"+"HadMeeting".Translate()+", [other_nameShortIndef].");
                    RuleStrings.SetValue(goodbyeTextRecip, text2);
                    endConversation.logRulesInitiator = goodbyeTextInit;
                    endConversation.logRulesRecipient = goodbyeTextRecip;
                    FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
                    Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOf.DeepTalk));
                    PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, this.constituent, this.mayor, new List<RulePackDef>());
                    Find.PlayLog.Add(log);
                    MoteMaker.MakeInteractionBubble(this.mayor, this.constituent, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.Symbol);
                }
            }
        }
        
        public override string GetReport()
        {
            return "LordReportVisitingMayor".Translate();
        }
        
        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(base.Map) || this.constituent.GetTimeAssignment() == TimeAssignmentDefOf.Work || this.mayor.GetTimeAssignment() == TimeAssignmentDefOf.Work || (!this.spot.Roofed(base.Map) && !JoyUtility.EnjoyableOutsideNow(base.Map, null));
        }
        
        private IntVec3 spot;
        private Trigger_TicksPassed timeoutTrigger;
        public Pawn constituent;
        public Pawn mayor;
        private bool complaint;
        public int ticksInSameRoom;
    }
}
