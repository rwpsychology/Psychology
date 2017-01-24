using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Grammar;
using System.Reflection;

namespace Psychology
{
    public class Hediff_Conversation : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            realPawn = pawn as PsychologyPawn;
            if (realPawn == null)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.otherPawn.Spawned || !this.pawn.Spawned || (this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 36f || !InteractionUtility.CanReceiveInteraction(this.pawn) || !InteractionUtility.CanReceiveInteraction(this.otherPawn) || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true))
            {
                pawn.health.RemoveHediff(this);
                return;
            }
            if(this.pawn.Dead || this.pawn.Downed)
            {
                pawn.health.RemoveHediff(this);
                return;
            }
            if(this.pawn.IsHashIntervalTick(200))
            {
                if (Rand.Value > 1f - (this.ageTicks / 50000f))
                {
                    pawn.health.RemoveHediff(this);
                    return;
                }
                else if (Rand.Value < 0.1f)
                {
                    MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOf.DeepTalk.interactionMote, InteractionDefOf.DeepTalk.Symbol);
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if(realPawn != null)
            {
                Log.Message(this.ageTicks.ToString());
                Hediff otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h.def.defName == "HoldingConversation" && ((Hediff_Conversation)h).otherPawn == this.realPawn);
                if(otherConvo != null)
                {
                    otherPawn.health.RemoveHediff(otherConvo);
                }
                string talkDesc;
                int talkLength;
                if (this.ageTicks < 200)
                {
                    talkDesc = shortTalkDescriptions.RandomElement();
                    talkLength = 0;
                }
                else if (this.ageTicks < 1500)
                {
                    talkDesc = normalTalkDescriptions.RandomElement();
                    talkLength = 1;
                }
                else if (this.ageTicks < 5000)
                {
                    talkDesc = longTalkDescriptions.RandomElement();
                    talkLength = 2;
                }
                else
                {
                    talkDesc = epicTalkDescriptions.RandomElement();
                    talkLength = 3;
                }
                talkDesc = talkDesc.Replace("{}", topic.def.conversationTopic);
                //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
                ThoughtDef def = new ThoughtDef();
                def.defName = this.pawn.GetHashCode() + "Conversation" + topic.def.defName + Find.TickManager.TicksGame;
                def.label = "conversation";
                def.durationDays = 60f;
                def.thoughtClass = typeof(Thought_MemorySocial);
                ThoughtStage stage = new ThoughtStage();
                //Base opinion mod is 5 to the power of controversiality.
                float opinionMod = Mathf.Pow(5f, topic.def.controversiality);
                //Multiplied by difference between their personality ratings, on an exponential scale.
                Log.Message("Personality modifier: " + Mathf.Lerp(-1.25f, 1.25f, Mathf.Pow(1f - Mathf.Abs((realPawn.psyche.GetPersonalityRating(topic.def) - otherPawn.psyche.GetPersonalityRating(topic.def))), 3)));
                opinionMod *= Mathf.Lerp(-1.25f, 1.25f, Mathf.Pow(1f-Mathf.Abs((realPawn.psyche.GetPersonalityRating(topic.def) - otherPawn.psyche.GetPersonalityRating(topic.def))),3));
                //The length of the talk has a large impact on how much the pawn cares.
                opinionMod *= talkModifiers[talkLength];
                //If they had a bad experience, the more polite the pawn is, the less they're bothered by it.
                opinionMod *= (opinionMod < 0f ? 0.5f + (1f - this.otherPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                //The more judgmental the pawn, the more they're affected by all conversations.
                opinionMod *= 0.5f + this.realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental);
                if(opinionMod < 0f)
                {
                    opinionMod *= PopulationModifier;
                }
                stage.label = "conversation about " + topic.def.conversationTopic;
                stage.baseOpinionOffset = Mathf.RoundToInt(opinionMod);
                def.stages.Add(stage);
                Log.Message(realPawn.LabelShort + " had convo with " + otherPawn.LabelShort + " about " + topic.def.conversationTopic + " and got " + opinionMod + " opinion offset.");
                this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(def, otherPawn);
                if (this.waveGoodbye)
                {
                    InteractionDef endConversation = new InteractionDef();
                    FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
                    endConversation.defName = "EndConversation" + topic.def.defName + talkLength + Find.TickManager.TicksGame;
                    RulePack goodbyeText = new RulePack();
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    List<string> text = new List<string>();
                    text.Add("logentry->" + talkDesc);
                    RuleStrings.SetValue(goodbyeText, text);
                    endConversation.logRulesInitiator = goodbyeText;
                    endConversation.logRulesRecipient = goodbyeText;
                    Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOf.DeepTalk));
                    PlayLogEntry_Interaction log = new PlayLogEntry_Interaction(endConversation, realPawn, otherPawn, new List<RulePackDef>());
                    Find.PlayLog.Add(log);
                    MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.Symbol);
                }
            }
        }

        public float PopulationModifier
        {
            get
            {
                return Mathf.Clamp01(this.pawn.Map.mapPawns.FreeColonistsCount / 8f);
            }
        }

        private float[] talkModifiers = { 0.1f, 1f, 2f, 5f };
        private string[] shortTalkDescriptions = { "Had a brief chat with [other_nameShortIndef] about {}.", "Had a short talk with [other_nameShortIndef] about {}.", "Quickly discussed {} with [other_nameShortIndef].", "Fleetingly chatted about {} with [other_nameShortIndef]." };
        private string[] normalTalkDescriptions = { "Talked about {} with [other_nameShortIndef].", "Had a conversation about {} with [other_nameShortIndef].", "Discussed {} with [other_nameShortIndef].", "Compared thoughts on {} with [other_nameShortIndef].", "Conversed with [other_nameShortIndef] about {}.", "Talked with [other_nameShortIndef] about {}.", "Had a discussion with [other_nameShortIndef] about {}." };
        private string[] longTalkDescriptions = { "Discussed {} at length with [other_nameShortIndef].", "Had a long conversation about {} with [other_nameShortIndef].", "Talked with [other_nameShortIndef] about {} for some time.", "Thoroughly discussed {} with [other_nameShortIndef].", "Argued with {} about [other_nameShortIndef]." };
        private string[] epicTalkDescriptions = { "Conferred with [other_nameShortIndef] for hours about {}.", "Talked endlessly with [other_nameShortIndef] about {}.", "Had an extremely long conversation about {} with [other_nameShortIndef].", "Debated {} to death with [other_nameShortIndef]." };
        public PsychologyPawn realPawn;
        public PsychologyPawn otherPawn;
        public PersonalityNode topic;
        public bool waveGoodbye;
    }
}
