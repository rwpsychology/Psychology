using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class InteractionWorker_Conversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            if(realRecipient == null || recipient.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation))
            {
                return 0f;
            }
            return 0.75f * realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly);
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            if(realInitiator != null && realRecipient != null)
            {
                PersonalityNode topic = realInitiator.psyche.PersonalityNodes.Where(node => !node.Core).RandomElement();
                Hediff_Conversation initiatorHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, realInitiator);
                initiatorHediff.otherPawn = realRecipient;
                initiatorHediff.topic = topic;
                initiatorHediff.waveGoodbye = true;
                initiator.health.AddHediff(initiatorHediff);
                Hediff_Conversation recipientHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, realRecipient);
                recipientHediff.otherPawn = realInitiator;
                recipientHediff.topic = topic;
                recipientHediff.waveGoodbye = false;
                recipient.health.AddHediff(recipientHediff);
            }
        }
        
    }
}
