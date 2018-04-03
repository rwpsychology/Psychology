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
    public class InteractionWorker_Conversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realRecipient == null || realInitiator == null)
            {
                return 0f;
            }
            if (!initiator.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || !recipient.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return 0f;
            }
            float baseChance = 0.45f;
            Lord lord = LordUtility.GetLord(initiator);
            if (lord != null && (lord.LordJob.GetType() == typeof(LordJob_HangOut) || lord.LordJob.GetType() == typeof(LordJob_Date)) && LordUtility.GetLord(recipient) == lord)
            {
                baseChance = 0.75f;
            }
            return Mathf.Max(0f, baseChance + (realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly)-0.6f) + (realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted)-0.5f));
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PersonalityNode topic = (from node in realInitiator.psyche.PersonalityNodes
                                     where !node.Core
                                     select node).RandomElementByWeight(node => realInitiator.psyche.GetConversationTopicWeight(node.def, realRecipient));
            Hediff_Conversation initiatorHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, realInitiator);
            initiatorHediff.otherPawn = realRecipient;
            initiatorHediff.topic = topic.def;
            initiatorHediff.waveGoodbye = true;
            realInitiator.health.AddHediff(initiatorHediff);
            Hediff_Conversation recipientHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, realRecipient);
            recipientHediff.otherPawn = realInitiator;
            recipientHediff.topic = topic.def;
            recipientHediff.waveGoodbye = false;
            realRecipient.health.AddHediff(recipientHediff);
        }
        
    }
}
