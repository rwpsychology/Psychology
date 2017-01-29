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
            transition.AddTrigger(new Trigger_PawnLostViolently());
            stateGraph.AddTransition(transition);
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 15000));
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
            Scribe_Values.LookValue(ref this.spot, "spot", default(IntVec3));
            Scribe_References.LookReference(ref this.constituent, "constituent");
            Scribe_References.LookReference(ref this.mayor, "mayor");
            Scribe_Values.LookValue(ref this.complaint, "complaining");
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
                        complaintDef.thoughtClass = typeof(Thought_MemoryDynamic);
                        complaintDef.stackedEffectMultiplier = 1f;
                        ThoughtStage complaintStage = new ThoughtStage();
                        float complaintMood = 10f * (realMayor.psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic) - 0.25f);
                        complaintMood *= this.ticksInSameRoom / 5000f;
                        complaintMood *= (complaintMood < 0f ? 0.5f + (1f - realMayor.psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                        complaintMood += (BeautyUtility.AverageBeautyPerceptible(this.constituent.Position, this.constituent.Map) / 10f);
                        complaintMood *= 0.75f + (realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental)/2f);
                        complaintStage.label = "complained to the mayor";
                        complaintStage.description = "Complaining to the mayor made me feel this way.";
                        complaintStage.baseMoodEffect = Mathf.RoundToInt(complaintMood);
                        complaintDef.defName = this.constituent.GetHashCode() + "MayorComplaint" + complaintStage.baseMoodEffect;
                        complaintDef.stages.Add(complaintStage);
                        this.constituent.needs.mood.thoughts.memories.TryGainMemoryThought(complaintDef, this.mayor);
                    }
                    ThoughtDef visitDef = new ThoughtDef();
                    visitDef.label = "MayorVisited";
                    visitDef.durationDays = 0.75f + 2f * (1f - realMayor.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent));
                    visitDef.thoughtClass = typeof(Thought_MemoryDynamic);
                    visitDef.stackedEffectMultiplier = 1f;
                    ThoughtStage stage = new ThoughtStage();
                    float mood = 5f;
                    mood *= this.ticksInSameRoom / 5000f;
                    mood *= (complaint ? -1f-(1f-this.constituent.needs.mood.CurLevel) : 0.25f+Mathf.Max(0f, 0.25f-this.constituent.needs.mood.CurLevel));
                    mood *= (mood < 0f ? 0.5f + (1f - realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                    mood *= 0.5f + (1f - realConstituent.psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack));
                    stage.label = "visited by constituent";
                    stage.description = "A visit from a constituent made me feel this way.";
                    stage.baseMoodEffect = Mathf.RoundToInt(mood);
                    visitDef.defName = this.mayor.GetHashCode() + "MayorVisited" + stage.baseMoodEffect;
                    visitDef.stages.Add(stage);
                    this.mayor.needs.mood.thoughts.memories.TryGainMemoryThought(visitDef, this.constituent);
                    InteractionDef endConversation = new InteractionDef();
                    endConversation.defName = "EndConversation";
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    RulePack goodbyeTextInit = new RulePack();
                    List<string> text = new List<string>(1);
                    if (complaint)
                    {
                        text.Add("logentry->Complained to the mayor.");
                    }
                    else
                    {
                        text.Add("logentry->Voiced my support to the mayor.");
                    }
                    RuleStrings.SetValue(goodbyeTextInit, text);
                    RulePack goodbyeTextRecip = new RulePack();
                    List<String> text2 = new List<string>(1);
                    text2.Add("logentry->Had a meeting with a constituent, [other_nameShortIndef].");
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
        
        private bool IsPartyAboutToEnd()
        {
            return this.timeoutTrigger.TicksLeft < 1200;
        }
        
        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableMapConditionsToContinueParty(base.Map) || (!this.spot.Roofed(base.Map) && !JoyUtility.EnjoyableOutsideNow(base.Map, null));
        }
        
        private IntVec3 spot;
        private Trigger_TicksPassed timeoutTrigger;
        public Pawn constituent;
        public Pawn mayor;
        private bool complaint;
        public int ticksInSameRoom;
    }
}
