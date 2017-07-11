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
    public class InteractionWorker_PlanDate : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if(!initiator.IsColonist || !recipient.IsColonist)
            {
                return 0f;
            }
            if(!PartyUtility.AcceptableGameConditionsToStartParty(initiator.Map))
            {
                return 0f;
            }
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realRecipient == null || realInitiator == null)
            {
                return 0f;
            }
            if(!LovePartnerRelationUtility.LovePartnerRelationExists(initiator,recipient))
            {
                return 0f;
            }
            if(realInitiator.psyche.lastDateTick >= Find.TickManager.TicksGame - GenDate.TicksPerDay*7 || realRecipient.psyche.lastDateTick >= Find.TickManager.TicksGame - GenDate.TicksPerDay * 7)
            {
                return 0f;
            }
            if(initiator.health.summaryHealth.SummaryHealthPercent < 1f || recipient.health.summaryHealth.SummaryHealthPercent < 1f)
            {
                return 0f;
            }
            return 1.2f * realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (1f - realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent))
                * realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (1f - realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) * RendezvousUtility.ColonySizeFactor(initiator);
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            PsychologyPawn realRecipient = recipient as PsychologyPawn;
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            //Choose a time that works with their schedule, based on their personality
            Dictionary<int, float> possibleHours = new Dictionary<int, float>();
            for (int i = 0; i < GenDate.HoursPerDay; i++)
            {
                possibleHours.Add(i, 0f);
            }
            foreach (PersonalityNodeDef d in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
            {
                if(d.preferredDateHours != null)
                {
                    foreach (int h in d.preferredDateHours)
                    {
                        possibleHours[h] += (Mathf.Pow(Mathf.Abs(0.5f - realInitiator.psyche.GetPersonalityRating(d)), 2) / (1.3f - realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)) + Mathf.Pow(Mathf.Abs(0.5f - realRecipient.psyche.GetPersonalityRating(d)), 2) / (1.3f - realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)) / 2f);
                    }
                }
            }
            int hour = possibleHours.Keys.RandomElementByWeight(h => possibleHours[h] * RendezvousUtility.TimeAssignmentFactor(initiator, h) * RendezvousUtility.TimeAssignmentFactor(recipient, h));
            //More Spontaneous couples will plan their dates sooner; possibly even immediately!
            int day = Find.TickManager.TicksGame + Mathf.RoundToInt(GenDate.TicksPerDay * 5 * (((1f - realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous)) + (1f - realRecipient.psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous))) / 2f));
            Hediff_PlannedDate plannedDate = HediffMaker.MakeHediff(HediffDefOfPsychology.PlannedDate, initiator) as Hediff_PlannedDate;
            plannedDate.partner = recipient;
            plannedDate.day = day;
            plannedDate.hour = hour;
            initiator.health.AddHediff(plannedDate);
            realInitiator.psyche.lastDateTick = day;
            realRecipient.psyche.lastDateTick = day;
            if (Prefs.DevMode && Prefs.LogVerbose)
            {
                Log.Message(initiator.LabelShort + " planned date with " + recipient.LabelShort + " for hour " + hour + " on date " + GenDate.DateFullStringAt(day, Find.WorldGrid.LongLatOf(initiator.Map.Tile)));
            }
        }
    }
}
