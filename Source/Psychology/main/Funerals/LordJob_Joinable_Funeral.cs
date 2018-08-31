using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    class LordJob_Joinable_Funeral: LordJob_VoluntarilyJoinable
    {
        public LordJob_Joinable_Funeral()
        { }
        
        public LordJob_Joinable_Funeral(IntVec3 spot)
        {
            this.spot = spot;
        }

        public LordJob_Joinable_Funeral(IntVec3 spot, Building_Grave buried)
        {
            this.spot = spot;
            this.dead = buried.Corpse.InnerPawn;
            this.grave = buried;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_Funeral lordToil_Funeral = new LordToil_Funeral(this.spot);
            stateGraph.AddToil(lordToil_Funeral);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_Funeral, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => !PartyUtility.AcceptableGameConditionsToContinueParty(base.Map)));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddPreAction(new TransitionAction_Message("MessageFuneralCalledOff".Translate(), MessageTypeDefOf.NegativeEvent, new TargetInfo(this.spot, this.Map, false)));
            stateGraph.AddTransition(transition);
            int length = Rand.RangeInclusive(GenDate.TicksPerHour, Mathf.RoundToInt(GenDate.TicksPerHour * 2.5f));
            this.timeoutTrigger = new Trigger_TicksPassed(length);
            Transition transition2 = new Transition(lordToil_Funeral, lordToil_End);
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
            Scribe_Values.Look(ref spot, "spot", default(IntVec3));
            Scribe_References.Look(ref grave, "grave");
            Scribe_Collections.Look(ref attendees, "attendees", LookMode.Reference);
        }
        
        private void Finished()
        {
            StringBuilder attendedString = new StringBuilder();
            foreach(Pawn p in attendees)
            {
                attendedString.AppendLine(p.Name.ToString());
                if (PsycheHelper.PsychologyEnabled(p))
                {
                    ThoughtDef def = new ThoughtDef();
                    def.defName = p.GetHashCode() + "AttendedFuneral" + dead.GetHashCode();
                    def.durationDays = 20f;
                    def.nullifyingTraits = new List<TraitDef>();
                    def.nullifyingTraits.Add(TraitDefOf.Psychopath);
                    def.nullifyingTraits.Add(TraitDefOfPsychology.Desensitized);
                    def.thoughtClass = typeof(Thought_MemoryDynamic);
                    ThoughtStage stage = new ThoughtStage();
                    stage.label = "AttendedFuneralThought".Translate(dead);
                    stage.baseMoodEffect = Mathf.RoundToInt((p.relations.OpinionOf(dead)/15f) * (0.33f + PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Nostalgic)));
                    stage.description = "AttendedFuneralDesc".Translate().AdjustedFor(dead);
                    def.stages.Add(stage);
                    p.needs.mood.thoughts.memories.TryGainMemory(def);
                }
                else
                {
                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.AttendedFuneral);
                }
            }
            if(attendees.Count == 0)
            {
                attendedString.AppendLine("No one.");
            }
            Find.LetterStack.ReceiveLetter("LetterLabelFuneralEnded".Translate(dead), "LetterFuneralEnded".Translate(dead, attendedString), LetterDefOf.NeutralEvent, null);
        }
        
        public override string GetReport()
        {
            return "LordReportAttendingFuneral".Translate();
        }
        
        private bool IsInvited(Pawn p)
        {
            return p.Faction == this.lord.faction && p.needs.mood != null && p.relations.OpinionOf(dead) > 0 && (!PsycheHelper.PsychologyEnabled(p) || Mathf.Lerp(0, 100, p.relations.OpinionOf(dead)) >= (1f - PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Nostalgic)));
        }
        
        private bool IsPartyAboutToEnd()
        {
            return this.timeoutTrigger.TicksLeft < GenDate.TicksPerHour/8;
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (!this.IsInvited(p))
            {
                return 0f;
            }
            if (!GatheringsUtility.ShouldGuestKeepAttendingGathering(p))
            {
                return 0f;
            }
            if (!this.lord.ownedPawns.Contains(p) && this.IsPartyAboutToEnd())
            {
                return 0f;
            }
            return 20f;
        }

        public void Attended(Pawn p)
        {
            if(!attendees.Contains(p))
            {
                attendees.Add(p);
            }
        }
        
        private IntVec3 spot;
        private Pawn dead;
        private Building_Grave grave;
        private Trigger_TicksPassed timeoutTrigger;
        private List<Pawn> attendees = new List<Pawn>();
    }
}
