using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
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
            return Mathf.Max(0f, 0.45f + (0.6f-realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly)) + (0.5f-realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted)));
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PersonalityNode topic = realInitiator.psyche.PersonalityNodes.Where(node => !node.Core).RandomElement();
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
