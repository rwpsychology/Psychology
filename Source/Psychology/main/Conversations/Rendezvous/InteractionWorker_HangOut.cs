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
            if(!GatheringsUtility.ShouldGuestKeepAttendingGathering(initiator) || !GatheringsUtility.ShouldGuestKeepAttendingGathering(recipient))
            {
                return 0f;
            }
            if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
            {
                return 0f;
            }
            if (initiator.GetLord() != null || recipient.GetLord() != null)
            {
                return 0f;
            }
            if (initiator.Drafted || recipient.Drafted)
            {
                return 0f;
            }
            if (!RendezvousUtility.AcceptableGameConditionsToStartHangingOut(initiator.Map))
            {
                return 0f;
            }
            if (initiator.Faction != recipient.Faction)
            {
                return 0f;
            }
            float initiatorFactor = 0f;
            float recipientFactor = 0f;
            if (initiator.relations.OpinionOf(recipient) > -20)
            {
                initiatorFactor = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted) + 0.15f + Mathf.InverseLerp(0f, 100f, initiator.relations.OpinionOf(recipient));
                recipientFactor = (PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly) + PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool))/2f;
            }
            else if(initiator.relations.OpinionOf(recipient) <= -20)
            {
                initiatorFactor = Mathf.InverseLerp(0.6f, 1f, PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic));
                recipientFactor = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Trusting);
            }
            float scheduleFactor = 0f;
            if (initiator.GetTimeAssignment() == TimeAssignmentDefOf.Anything)
            {
                scheduleFactor = 0.33f;
            }
            else if (initiator.GetTimeAssignment() == TimeAssignmentDefOf.Joy)
            {
                scheduleFactor = 1f;
            }
            if (initiator.mindState.IsIdle && recipient.mindState.IsIdle && initiator.GetTimeAssignment() != TimeAssignmentDefOf.Work && recipient.GetTimeAssignment() != TimeAssignmentDefOf.Work)
            {
                scheduleFactor = 5f;
            }
            return 0.05f * initiatorFactor * recipientFactor * scheduleFactor * RendezvousUtility.ColonySizeFactor(initiator);
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            initiator.jobs.StopAll();
            recipient.jobs.StopAll();
            Lord meeting = LordMaker.MakeNewLord(initiator.Faction, new LordJob_HangOut(initiator, recipient), initiator.Map, new Pawn[] { initiator, recipient });
        }
        
    }
}
