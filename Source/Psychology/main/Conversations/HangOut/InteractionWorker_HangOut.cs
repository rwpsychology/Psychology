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
    public class InteractionWorker_HangOut : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realRecipient == null || realInitiator == null)
            {
                return 0f;
            }
            if (initiator.GetLord() != null || recipient.GetLord() != null)
            {
                return 0f;
            }
            float initiatorFactor = 0f;
            float recipientFactor = 0f;
            if (initiator.relations.OpinionOf(recipient) > -20)
            {
                initiatorFactor = realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted) + 0.15f + Mathf.InverseLerp(0f, 100f, initiator.relations.OpinionOf(recipient));
                recipientFactor = realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly);
            }
            else if(initiator.relations.OpinionOf(recipient) <= -20)
            {
                initiatorFactor = Mathf.InverseLerp(0f, 0.5f, realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic)-0.5f);
                recipientFactor = realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Trusting);
            }
            return 0.05f * initiatorFactor * recipientFactor * Mathf.Clamp01(initiator.Map.mapPawns.FreeColonistsCount / 8f);
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            List<Pawn> pawns = new List<Pawn>();
            pawns.Add(initiator);
            pawns.Add(recipient);
            Lord meeting = LordMaker.MakeNewLord(initiator.Faction, new LordJob_HangOut(initiator, recipient), initiator.Map, pawns);
        }
        
    }
}
