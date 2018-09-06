using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Building_Grave), nameof(Building_Grave.Notify_CorpseBuried))]
    public static class Building_Grave_NotifyCorpseBuried_Patch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void FillGraveThought(Building_Grave __instance, Pawn worker)
        {
            if (worker.needs.mood != null)
            {
                worker.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.FilledGraveBleedingHeart);
            }
        }
    }

    [HarmonyPatch(typeof(Building_Grave), nameof(Building_Grave.Notify_CorpseBuried))]
    public static class Building_Grave_NotifyCorpseBuriedFuneralHook
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void PlanFuneral(Building_Grave __instance, Pawn worker)
        {
            Pawn planner;
            (from c in worker.Map.mapPawns.FreeColonistsSpawned
             where c.relations.OpinionOf(__instance.Corpse.InnerPawn) >= 20
             select c).TryRandomElementByWeight((c) => Mathf.Max(0f, c.relations.OpinionOf(__instance.Corpse.InnerPawn) - (PsycheHelper.PsychologyEnabled(c) ? 100f * (1f - PsycheHelper.Comp(c).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Nostalgic)) : 0f)), out planner);
            if(planner != null && PsycheHelper.PsychologyEnabled(__instance.Corpse.InnerPawn) && !PsycheHelper.Comp(__instance.Corpse.InnerPawn).AlreadyBuried)
            {
                Func<int, float> timeAssignmentFactor = delegate(int h)
                {
                    if (planner.timetable.GetAssignment(h) == TimeAssignmentDefOf.Joy)
                    {
                        return 1.25f;
                    }
                    if (planner.timetable.GetAssignment(h) == TimeAssignmentDefOf.Anything)
                    {
                        return 0.9f;
                    }
                    return 0f;
                };
                int hour = -1;
                if (Enumerable.Range(0, GenDate.HoursPerDay).TryRandomElementByWeight(h => timeAssignmentFactor(h), out hour))
                {
                    int date = Find.TickManager.TicksGame + Mathf.RoundToInt(GenDate.TicksPerDay * (2f + (PsycheHelper.PsychologyEnabled(planner) ? 3f - (5f * PsycheHelper.Comp(planner).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous)) : 0f)));
                    int currentDay = GenDate.DayOfYear(GenDate.TickGameToAbs(date), Find.WorldGrid.LongLatOf(planner.Map.Tile).x);
                    if (currentDay <= GenLocalDate.DayOfYear(planner.Map) && GenDate.HourOfDay(GenDate.TickGameToAbs(date), Find.WorldGrid.LongLatOf(planner.Map.Tile).x) > hour)
                    {
                        date += GenDate.TicksPerDay * (currentDay - GenLocalDate.DayOfYear(planner.Map));
                    }
                    Hediff_Funeral planFuneral = HediffMaker.MakeHediff(HediffDefOfPsychology.PlannedFuneral, planner) as Hediff_Funeral;
                    planFuneral.date = date;
                    planFuneral.hour = hour;
                    planFuneral.day = GenDate.DayOfYear(GenDate.TickGameToAbs(date), Find.WorldGrid.LongLatOf(planner.Map.Tile).x);
                    planFuneral.grave = __instance;
                    planFuneral.spot = __instance.Position;
                    planner.health.AddHediff(planFuneral);
                    PsycheHelper.Comp(__instance.Corpse.InnerPawn).AlreadyBuried = true;
                }
            }
        }
    }
}
