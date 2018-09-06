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
            if (this.pawn.GetComp<CompPsychology>() == null || !this.pawn.GetComp<CompPsychology>().isPsychologyPawn)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.otherPawn, "otherPawn");
            Scribe_Defs.Look(ref this.topic, "topic");
            Scribe_Values.Look(ref this.waveGoodbye, "waveGoodbye");
        }

        [LogPerformance]
        public override void Tick()
        {
            base.Tick();
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
                float mtb = 3f;
                if (this.ageTicks > GenDate.TicksPerHour*2)
                {
                    mtb = 0.5f;
                }
                else if (this.ageTicks > GenDate.TicksPerHour)
                {
                    mtb = 1f;
                }
                else if (this.ageTicks > GenDate.TicksPerHour/2)
                {
                    mtb = 2f;
                }
                if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
                {
                    mtb *= 2f;
                }
                if (this.otherPawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
                {
                    mtb *= 2f;
                }
                if (Rand.MTBEventOccurs(mtb, GenDate.TicksPerHour, 200))
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

        [LogPerformance]
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.pawn != null && this.otherPawn != null)
            {
                if (this.pawn.Dead || this.otherPawn.Dead || this.pawn.GetComp<CompPsychology>() == null || this.otherPawn.GetComp<CompPsychology>() == null || !this.pawn.GetComp<CompPsychology>().isPsychologyPawn || !this.otherPawn.GetComp<CompPsychology>().isPsychologyPawn)
                {
                    return;
                }
                Hediff otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h is Hediff_Conversation && ((Hediff_Conversation)h).otherPawn == this.pawn);
                if(otherConvo != null)
                {
                    this.otherPawn.health.RemoveHediff(otherConvo);
                }
                string talkDesc;
                if (this.ageTicks < 500)
                {
                    int numShortTalks = int.Parse("NumberOfShortTalks".Translate());
                    talkDesc = "ShortTalk" + Rand.RangeInclusive(1, numShortTalks);
                }
                else if (this.ageTicks < 1500)
                {
                    int numNormalTalks = int.Parse("NumberOfNormalTalks".Translate());
                    talkDesc = "NormalTalk" + Rand.RangeInclusive(1, numNormalTalks);
                }
                else if (this.ageTicks < 5000)
                {
                    int numLongTalks = int.Parse("NumberOfLongTalks".Translate());
                    talkDesc = "LongTalk" + Rand.RangeInclusive(1, numLongTalks);
                }
                else
                {
                    int numEpicTalks = int.Parse("NumberOfEpicTalks".Translate());
                    talkDesc = "EpicTalk" + Rand.RangeInclusive(1, numEpicTalks);
                }
                //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
                ThoughtDef def = new ThoughtDef();
                def.defName = this.pawn.GetHashCode() + "Conversation" + topic.defName;
                def.label = topic.defName;
                def.durationDays = PsychologyBase.ConvoDuration();
                def.nullifyingTraits = new List<TraitDef>();
                def.nullifyingTraits.Add(TraitDefOf.Psychopath);
                def.thoughtClass = typeof(Thought_MemorySocialDynamic);
                ThoughtStage stage = new ThoughtStage();
                //Base opinion mod is 5 to the power of controversiality.
                float opinionMod = Mathf.Pow(5f, topic.controversiality);
                //Multiplied by difference between their personality ratings, on an exponential scale.
                opinionMod *= Mathf.Lerp((LovePartnerRelationUtility.LovePartnerRelationExists(pawn,otherPawn) ? -2f : -1.5f), 1.25f, Mathf.Pow(1f-Mathf.Abs(this.pawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(topic) - this.otherPawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(topic)),3));
                //Cool pawns are liked more.
                opinionMod += Mathf.Pow(2f, topic.controversiality) * (0.5f - this.otherPawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool));
                //The length of the talk has a large impact on how much the pawn cares.
                opinionMod *= 5f * (this.ageTicks / (GenDate.TicksPerHour * 2.25f)); //talkModifier[talkLength]
                //If they had a bad experience, the more polite the pawn is, the less they're bothered by it.
                opinionMod *= (opinionMod < 0f ? 0.5f + (1f - this.otherPawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
                //The more judgmental the pawn, the more they're affected by all conversations.
                opinionMod *= 0.5f + this.pawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental);
                if (opinionMod < 0f)
                {
                    opinionMod *= PopulationModifier;
                }
                else if(LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.otherPawn) && this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
                {
                    opinionMod *= 1.25f;
                }
                stage.label = "ConversationStage".Translate() + " " + topic.conversationTopic;
                stage.baseOpinionOffset = Mathf.RoundToInt(opinionMod);
                def.stages.Add(stage);
                /* The more they know about someone, the less likely small thoughts are to have an impact on their opinion.
                 * This helps declutter the Social card without preventing pawns from having conversations.
                 * They just won't change their mind about the colonist as a result.
                 */
                if (Rand.Value < Mathf.InverseLerp(0f, this.pawn.GetComp<CompPsychology>().Psyche.TotalThoughtOpinion(this.otherPawn), 250f+Mathf.Abs(stage.baseOpinionOffset)) && stage.baseOpinionOffset != 0)
                {
                    this.pawn.needs.mood.thoughts.memories.TryGainMemory(def, this.otherPawn);
                }
                if (this.waveGoodbye && this.pawn.Map != null)
                {
                    InteractionDef endConversation = new InteractionDef();
                    endConversation.defName = "EndConversation";
                    RulePack goodbyeText = new RulePack();
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    List<string> text = new List<string>(1);
                    text.Add("r_logentry->" + talkDesc.Translate(topic.conversationTopic));
                    RuleStrings.SetValue(goodbyeText, text);
                    endConversation.logRulesInitiator = goodbyeText;
                    FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
                    Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOf.DeepTalk));
                    PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, pawn, this.otherPawn, new List<RulePackDef>());
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
        
        public Pawn otherPawn;
        public PersonalityNodeDef topic;
        public bool waveGoodbye;
    }
}
