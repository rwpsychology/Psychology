using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Grammar;
using System.Reflection;
using Harmony;

namespace Psychology
{
    public class Hediff_Conversation : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            if (!PsycheHelper.PsychologyEnabled(this.pawn))
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
            Scribe_Values.Look(ref this.convoTopic, "convoTopic", "something");
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
                /* When a conversation first starts, the mean time for it to last is 3 hours.
                 * When it reaches half an hour, the mean time for it to continue is 2 hours.
                 * When it reaches an hour, the mean time for it to continue is 1 hour.
                 * When it surpasses 2 hours, it will on average last for half an hour more.
                 * Conversations will thus usually not surpass 2 hours, and very rarely surpass 2 and a half hours, but are very likely to last up to an hour.
                 */
                float mtb = 3f;
                if (this.ageTicks > GenDate.TicksPerHour * 2)
                {
                    mtb = 0.5f;
                }
                else if (this.ageTicks > GenDate.TicksPerHour)
                {
                    mtb = 1f;
                }
                else if (this.ageTicks > (GenDate.TicksPerHour / 2))
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
                    MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOfPsychology.EndConversation.interactionMote, InteractionDefOfPsychology.EndConversation.Symbol);
                }
            }
        }

        [LogPerformance]
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (this.pawn != null && this.otherPawn != null)
            {
                if (this.pawn.Dead || this.otherPawn.Dead || !PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(otherPawn))
                {
                    return;
                }
                Hediff_Conversation otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h is Hediff_Conversation && ((Hediff_Conversation)h).otherPawn == this.pawn) as Hediff_Conversation;
                if (otherConvo != null)
                {
                    this.otherPawn.health.RemoveHediff(otherConvo);
                    this.startedFight = otherConvo.startedFight;
                }
                string talkDesc;
                if (this.ageTicks < 500)
                {
                    int numShortTalks = int.Parse("NumberOfShortTalks".Translate());
                    talkDesc = "ShortTalk" + Rand.RangeInclusive(1, numShortTalks);
                }
                else if (this.ageTicks < GenDate.TicksPerHour / 2)
                {
                    int numNormalTalks = int.Parse("NumberOfNormalTalks".Translate());
                    talkDesc = "NormalTalk" + Rand.RangeInclusive(1, numNormalTalks);
                }
                else if (this.ageTicks < GenDate.TicksPerHour * 2.5)
                {
                    int numLongTalks = int.Parse("NumberOfLongTalks".Translate());
                    talkDesc = "LongTalk" + Rand.RangeInclusive(1, numLongTalks);
                }
                else
                {
                    int numEpicTalks = int.Parse("NumberOfEpicTalks".Translate());
                    talkDesc = "EpicTalk" + Rand.RangeInclusive(1, numEpicTalks);
                }
                float opinionMod;
                ThoughtDef def = CreateSocialThought(out opinionMod);
                bool mattered = TryGainThought(def, Mathf.RoundToInt(opinionMod));
                InteractionDef endConversation = new InteractionDef();
                endConversation.socialFightBaseChance = 0.2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) * PopulationModifier * Mathf.InverseLerp(0f, -80f, opinionMod);
                endConversation.defName = "EndConversation";
                endConversation.label = def.label;
                List<RulePackDef> socialFightPacks = new List<RulePackDef>();
                if (otherConvo != null && (startedFight || (mattered && this.pawn.interactions.CheckSocialFightStart(endConversation, otherPawn))))
                {
                    if (startedFight)
                    {
                        socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoRecipientStarted);
                    }
                    else
                    {
                        socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoInitiatorStarted);
                    }
                    this.startedFight = true;
                    if (!this.waveGoodbye && otherConvo.convoLog != null && !otherConvo.startedFight)
                    {
                        //The main conversation hediff was the other conversation, and didn't start a fight, so we have to add the extra sentence in after the fact.
                        Traverse.Create(otherConvo.convoLog).Field("extraSentencePacks").GetValue<List<RulePackDef>>().AddRange(socialFightPacks);
                    }
                }
                if (this.waveGoodbye && this.pawn.Map != null)
                {
                    RulePack goodbyeText = new RulePack();
                    FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
                    List<string> text = new List<string>(1);
                    text.Add("r_logentry->" + talkDesc.Translate(convoTopic));
                    RuleStrings.SetValue(goodbyeText, text);
                    endConversation.logRulesInitiator = goodbyeText;
                    FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
                    Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOfPsychology.EndConversation));
                    PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, pawn, this.otherPawn, socialFightPacks);
                    Find.PlayLog.Add(log);
                    convoLog = log;
                    MoteMaker.MakeInteractionBubble(this.pawn, this.otherPawn, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.Symbol);
                }
            }
        }

        [LogPerformance]
        private ThoughtDef CreateSocialThought(out float opinionMod)
        {
            //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
            ThoughtDef def = new ThoughtDef();
            def.defName = this.pawn.GetHashCode() + "Conversation" + topic.defName;
            def.label = topic.defName;
            def.durationDays = PsychologyBase.ConvoDuration();
            def.nullifyingTraits = new List<TraitDef>();
            def.nullifyingTraits.Add(TraitDefOf.Psychopath);
            def.thoughtClass = typeof(Thought_MemorySocialDynamic);
            ThoughtStage stage = new ThoughtStage();
            /* Base opinion mod is 5 to the power of controversiality.
             * Controversiality varies from 0.7 (~3.09) to 1.5 (~11.18).
             */
            opinionMod = Mathf.Pow(5f, topic.controversiality);
            /* That opinion mod is weighted by the difference in their personality. 1 is identical, 0 is polar opposites.
             * Here's the math on how this works:
             * 
             * The weighting is on a heavily customized cubic curve. It looks like this: https://www.wolframalpha.com/input/?i=(9.5((x-0.5)%5E3))%2B(2x%2F3)%5E2-0.3+from+0+to+1
             * The maximum positive weight is 1.27x. The maximum negative weight is -1.4875x. The neutral weight (0x) is at an opinion diff of 0.706.
             * This means that to have a positive thought from a conversation, pawns need to have a <31% difference in personality on that topic.
             * However, since it's a cubic curve, the negative modifier builds gradually. Pawns will need a >79% difference in personality to have a -0.5x weight.
             * But they'll also need a <23% difference to have a 0.5x weight. Normal agreement is at ~0.962, and normal disagreement is at ~0.073.
             * Pawns are unlikely to have huge differences in opinion. There are more ways for them to be close to each other than far apart. Here's the proof: https://anydice.com/program/1177b4
             * The approximate likelihood for them to agree on something with this weighting is 49.89%.
             * 
             * Pawns' differences are exacerbated when they are in a relationship, but their similarities are also magnified.
             * The neutral conversation weight (0x) moves from 0.706 to 0.759, so the threshold for agreement is ~5% higher.
             * Normal disagreement moves from 0.073 to 0.127, and normal agreement moves from 0.962 to 0.946.
             * The maximum positive weight moves from 1.27x to 1.4986x. The maximum negative weight moves from -1.425x to -1.9875x.
             * The approximate likelihood for them to agree on something with this weighting is 42.63%.
             */
            float rawOpinionDiff = 1f - Mathf.Abs(PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(topic) - PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(topic));
            if(LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.otherPawn))
            {
                opinionMod *= (13.5f * (Mathf.Pow(rawOpinionDiff - 0.5f, 3))) + Mathf.Pow(((3f * rawOpinionDiff) / 9f), 2) - 0.3f;
            }
            else
            {
                opinionMod *= (9.5f * (Mathf.Pow(rawOpinionDiff - 0.5f, 3))) + Mathf.Pow(((2f * rawOpinionDiff) / 3f), 2) - 0.3f;
            }
            //Old cubic interpolation weighting.
            //opinionMod *= Mathf.Lerp((LovePartnerRelationUtility.LovePartnerRelationExists(pawn, otherPawn) ? -2f : -1.5f), (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, otherPawn) ? 1.5f : 1.25f), weightedOpinionDiff);
            /* The Cool modifier ranges from 3^(0.7) ~ 2.16 to 3^(1.5) ~ 5.2.
             * If a pawn is Cool, that modifier is a positive one added to all conversational thoughts. Otherwise, it's subtracted.
             * On average, Cool pawns will be liked better, and non-Cool pawns will be disliked more.
             */
            opinionMod += Mathf.Pow(3f, topic.controversiality) * (PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool) - 0.5f);
            /* The length of the talk has a large impact on how much the pawn cares.
             * A conversation is considered "full impact" at 1,125 ticks, or less than half an hour in-game.
             * A short talk (500 ticks or less) has a maximum 0.53x impact. The max opinion this could give (for non-lovers) is 7.5/-8.8.
             * A normal talk (half an hour or less) has a maximum 1.33x impact. The max opinion this could give is 18.9/-22.1.
             * A long talk (2.5 hours or less) has a maximum 6.67x impact. The max opinion this could give is 94.7/-110.9.
             * An epic talk (2.5+ hours) has no maximum impact, but after 2 hours the MTB to end the conversation becomes half an hour, so it's unlikely they will ever have an epic talk.
             * An average conversation is 1-2 hours, so on average the max opinion (not counting Cool modifier) is 37.9/-44.3 to 75.7/-88.7.
             * Again, it's unlikely the numbers will get that high. This is assuming identical or polar opposite personalities.
             */
            opinionMod *= 6f * ((float)this.ageTicks / (float)(GenDate.TicksPerHour * 2.25f));
            // Negative opinions are tempered by how Polite the other pawn is. An extremely impolite pawn will make a bad opinion 1.5x worse. A very polite pawn will make it half as bad.
            opinionMod *= (opinionMod < 0f ? 0.5f + (1f - PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite)) : 1f);
            // Positive opinions are bolstered by how Friendly the other pawn is. An extremely friendly pawn will make a positive opinion 1.5x better. A very unfriendly pawn will halve it.
            opinionMod *= (opinionMod > 0f ? 0.5f + PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly) : 1f);
            // The more Judgmental the pawn, the more they're affected by all conversations.
            opinionMod *= 0.5f + PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental);
            // In low-population colonies, pawns will put aside their differences.
            if (opinionMod < 0f)
            {
                opinionMod *= PopulationModifier;
            }
            else if (LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.otherPawn) && this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                //If it's a positive thought about their lover, Codependent pawns are always 1.25x as affected by it.
                opinionMod *= 1.25f;
            }
            stage.label = "ConversationStage".Translate() + " " + convoTopic;
            stage.baseOpinionOffset = Mathf.RoundToInt(opinionMod);
            def.stages.Add(stage);
            return def;
        }

        [LogPerformance]
        private bool TryGainThought(ThoughtDef def, int opinionOffset)
        {
            ThoughtStage stage = def.stages.First();
            IEnumerable<Thought_MemorySocialDynamic> convoMemories;
            /* The more they know about someone, the less likely small thoughts are to have an impact on their opinion.
             * This helps declutter the Social card without preventing pawns from having conversations.
             * They just won't change their mind about the colonist as a result.
             */
            if (Rand.Value < Mathf.InverseLerp(0f, PsycheHelper.Comp(pawn).Psyche.TotalThoughtOpinion(this.otherPawn, out convoMemories), 250f + Mathf.Abs(opinionOffset)) && opinionOffset != 0)
            {
                this.pawn.needs.mood.thoughts.memories.TryGainMemory(def, this.otherPawn);
                return true;
            }
            return false;
        }

        public float PopulationModifier
        {
            get
            {
                if (this.pawn.IsColonist && this.pawn.Map != null)
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
        public string convoTopic;
        public bool waveGoodbye;
        public bool startedFight = false;
        public PlayLogEntry_InteractionConversation convoLog;
    }
}
