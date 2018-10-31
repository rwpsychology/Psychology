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
        [LogPerformance]
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if(!initiator.IsColonist || !recipient.IsColonist)
            {
                return 0f;
            }
            if(!RendezvousUtility.AcceptableGameConditionsToStartHangingOut(initiator.Map))
            {
                return 0f;
            }
            if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
            {
                return 0f;
            }
            if(!LovePartnerRelationUtility.LovePartnerRelationExists(initiator,recipient))
            {
                return 0f;
            }
            if(PsycheHelper.Comp(initiator).Psyche.lastDateTick >= Find.TickManager.TicksGame - GenDate.TicksPerDay*7 || PsycheHelper.Comp(recipient).Psyche.lastDateTick >= Find.TickManager.TicksGame - GenDate.TicksPerDay * 7)
            {
                return 0f;
            }
            if(!GatheringsUtility.ShouldGuestKeepAttendingGathering(initiator) || !GatheringsUtility.ShouldGuestKeepAttendingGathering(recipient))
            {
                return 0f;
            }
            return 1.2f * (1f + Mathf.InverseLerp(100f, 0f,initiator.needs.mood.thoughts.TotalOpinionOffset(recipient))) * PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (1f - PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent))
                * PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * (1f - PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) * RendezvousUtility.ColonySizeFactor(initiator);
        }

        [LogPerformance]
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
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
                        possibleHours[h] += (Mathf.Pow(Mathf.Abs(0.5f - PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(d)), 2) / (1.3f - PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)) + Mathf.Pow(Mathf.Abs(0.5f - PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(d)), 2) / (1.3f - PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive)) / 2f);
                    }
                }
            }
            int hour = possibleHours.Keys.RandomElementByWeight(h => possibleHours[h] * RendezvousUtility.TimeAssignmentFactor(initiator, h) * RendezvousUtility.TimeAssignmentFactor(recipient, h));
            //More Spontaneous couples will plan their dates sooner; possibly even immediately!
            int day = Find.TickManager.TicksAbs + Mathf.RoundToInt(GenDate.TicksPerDay * 5 * (((1f - PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous)) + (1f - PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous))) / 2f));
            Hediff_PlannedDate plannedDate = HediffMaker.MakeHediff(HediffDefOfPsychology.PlannedDate, initiator) as Hediff_PlannedDate;
            plannedDate.partner = recipient;
            plannedDate.day = day;
            plannedDate.hour = hour;
            initiator.health.AddHediff(plannedDate);
            PsycheHelper.Comp(initiator).Psyche.lastDateTick = day;
            PsycheHelper.Comp(recipient).Psyche.lastDateTick = day;
            if(PsychologyBase.SendDateLetters())
            {
                int hourDiscrepancy = GenDate.HourOfDay(day, Find.WorldGrid.LongLatOf(initiator.Map.Tile).x) - hour;
                int accurateTime = day + (Math.Abs(hourDiscrepancy) * GenDate.TicksPerHour);
                String dateTime = GenDate.QuadrumDateStringAt(accurateTime, Find.WorldGrid.LongLatOf(initiator.Map.Tile).x);
                Letter dateLetter = LetterMaker.MakeLetter("LetterLabelDatePlanned".Translate(), TranslatorFormattedStringExtensions.Translate("LetterDatePlanned", initiator, recipient, dateTime, hour), LetterDefOf.PositiveEvent);
                Find.LetterStack.ReceiveLetter(dateLetter);
            }
        }
    }
}
