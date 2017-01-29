using System;
using System.Collections.Generic;
using System.Linq;
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
            this.realPawn = pawn as PsychologyPawn;
            if (this.realPawn == null)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref this.otherPawn, "otherPawn");
            Scribe_Defs.LookDef(ref this.topic, "topic");
            Scribe_Values.LookValue(ref this.waveGoodbye, "waveGoodbye");
        }

        public override void Tick()
        {
            base.Tick();
            if(this.realPawn == null)
            {
                this.realPawn = this.pawn as PsychologyPawn;
            }
            if (this.otherPawn == null)
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            if (!this.otherPawn.Spawned || !this.pawn.Spawned || !InteractionUtility.CanReceiveInteraction(this.pawn) || !InteractionUtility.CanReceiveInteraction(this.otherPawn))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            if ((this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            if (this.otherPawn.Dead || this.otherPawn.Downed || this.otherPawn.InAggroMentalState)
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            if (this.pawn.IsHashIntervalTick(200))
            {
                if (Rand.Value > 1f - (this.ageTicks / 500000f))
                {
                    this.pawn.health.RemoveHediff(this);
                    return;
                }
                else if (Rand.Value < 0.2f && this.pawn.Map != null)
                {
                    MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOf.DeepTalk.interactionMote, InteractionDefOf.DeepTalk.Symbol);
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.realPawn == null)
            {
                this.realPawn = this.pawn as PsychologyPawn;
            }
            if (this.realPawn != null && this.otherPawn != null)
            {
                Hediff otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h is Hediff_Conversation && ((Hediff_Conversation)h).otherPawn == this.realPawn);
                if(otherConvo != null)
                {
                    this.otherPawn.health.RemoveHediff(otherConvo);
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
                talkDesc = string.Format(talkDesc, topic.conversationTopic);
                //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
                ThoughtDef def = new ThoughtDef();
                def.defName = this.pawn.GetHashCode() + "Conversation" + topic.defName + Find.TickManager.TicksGame;
                def.label = topic.defName;
                def.durationDays = 60f;
                def.nullifyingTraits = new List<TraitDef>();
                def.nullifyingTraits.Add(TraitDefOf.Psychopath);
                def.thoughtClass = typeof(Thought_MemorySocialDynamic);
                ThoughtStage stage = new ThoughtStage();
                //Base opinion mod is 5 to the power of controversiality.
                float opinionMod = Mathf.Pow(5f, topic.controversiality);
                //Multiplied by difference between their personality ratings, on an exponential scale.
                opinionMod *= Mathf.Lerp(-1.25f, 1.25f, Mathf.Pow(1f-Mathf.Abs((this.realPawn.psyche.GetPersonalityRating(topic) - this.otherPawn.psyche.GetPersonalityRating(topic))),3));
                //Cool pawns are liked more.
                opinionMod += Mathf.Pow(2f, topic.controversiality) * (0.5f - this.otherPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool));
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
                stage.label = "conversation about " + topic.conversationTopic;
                stage.baseOpinionOffset = Mathf.RoundToInt(opinionMod);
                def.stages.Add(stage);
                /* The more they know about someone, the less likely small thoughts are to have an impact on their opinion.
                 * This helps declutter the Social card without preventing pawns from having conversations.
                 * They just won't change their mind about the colonist as a result.
                 */
                float knownThoughtOpinion = 0f;
                this.realPawn.needs.mood.thoughts.memories.Memories.Where(m => m.def.defName.Contains("Conversation") && m.otherPawn.ThingID == this.otherPawn.ThingID).ToList().ForEach(m => knownThoughtOpinion += Mathf.Abs(m.CurStage.baseOpinionOffset));
                if(Rand.Value < Mathf.InverseLerp(0f, knownThoughtOpinion+1, 250f+Mathf.Abs(stage.baseOpinionOffset)))
                {
                    this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(def, this.otherPawn);
                }
                if (this.waveGoodbye)
                {
                    InteractionDef endConversation = new InteractionDef();
                    endConversation.defName = "EndConversation";
                    RulePack goodbyeText = new RulePack();
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    List<string> text = new List<string>(1);
                    text.Add("logentry->" + talkDesc);
                    RuleStrings.SetValue(goodbyeText, text);
                    endConversation.logRulesInitiator = goodbyeText;
                    endConversation.logRulesRecipient = goodbyeText;
                    FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
                    Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOf.DeepTalk));
                    PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, realPawn, this.otherPawn, new List<RulePackDef>());
                    Find.PlayLog.Add(log);
                    MoteMaker.MakeInteractionBubble(this.pawn, this.otherPawn, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.Symbol);
                }
            }
        }

        public float PopulationModifier
        {
            get
            {
                if(this.pawn.IsColonist && this.pawn.Map != null)
                {
                    return Mathf.Clamp01(this.pawn.Map.mapPawns.FreeColonistsCount / 8f);
                }
                else
                {
                    return 1f;
                }
            }
        }

        private float[] talkModifiers = { 0.1f, 1f, 2f, 5f };
        private string[] shortTalkDescriptions = { "Had a brief chat with [other_nameShortIndef] about {0}.", "Had a short chat with [other_nameShortIndef] about {0}.", "Had a short talk with [other_nameShortIndef] about {0}.",
            "Quickly discussed {0} with [other_nameShortIndef].", "Fleetingly chatted about {0} with [other_nameShortIndef].", "Talked about {0} with [other_nameShortIndef] in passing.",
            "Commented pithily on {0} with [other_nameShortIndef].", "Remarked on {0} with [other_nameShortIndef].", "Exchanged words with [other_nameShortIndef] about {0} momentarily.",
            "Had a brief exchange with [other_nameShortIndef] about {0}."};
        private string[] normalTalkDescriptions = { "Talked about {0} with [other_nameShortIndef].", "Had a conversation about {0} with [other_nameShortIndef].", "Discussed {0} with [other_nameShortIndef].",
            "Compared thoughts on {0} with [other_nameShortIndef].", "Conversed with [other_nameShortIndef] about {0}.", "Talked with [other_nameShortIndef] about {0}.", "Had a discussion with [other_nameShortIndef] about {0}.",
            "Learned about [other_nameShortIndef]'s opinions on {0}.", "Shared thoughts on {0} with [other_nameShortIndef].", "Learned about [other_nameShortIndef]'s thoughts on {0}.", "Argued about {0} with [other_nameShortIndef].",
            "Consulted with [other_nameShortIndef] about {0}."};
        private string[] longTalkDescriptions = { "Discussed {0} at length with [other_nameShortIndef].", "Had a long conversation about {0} with [other_nameShortIndef].",
            "Talked with [other_nameShortIndef] about {0} for some time.", "Thoroughly discussed {0} with [other_nameShortIndef].", "Consulted extensively about {0} with [other_nameShortIndef].",
            "Learned a lot about how [other_nameShortIndef] feels about {0}.", "Shared deep thoughts on {0} with [other_nameShortIndef].", "Had a lengthy debate about {0} with [other_nameShortIndef]."};
        private string[] epicTalkDescriptions = { "Conferred with [other_nameShortIndef] for hours about {0}.", "Talked endlessly with [other_nameShortIndef] about {0}.",
            "Had an extremely long conversation about {0} with [other_nameShortIndef].", "Debated forever with [other_nameShortIndef] about {0}.",
            "Had a very lengthy back-and-forth about {0} with [other_nameShortIndef].", "Vented about {0} with [other_nameShortIndef] for hours."};
        public PsychologyPawn realPawn;
        public PsychologyPawn otherPawn;
        public PersonalityNodeDef topic;
        public bool waveGoodbye;
    }
}
