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
    [HarmonyPatch(typeof(Building_Grave), "Notify_CorpseBuried")]
    public static class Building_Grave_NotifyCorpseBuried_Patch
    {
        [HarmonyPostfix]
        public static void FillGraveThought(Building_Grave __instance, Pawn worker)
        {
            if (worker.needs.mood != null)
            {
                worker.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.FilledGraveBleedingHeart);
            }
        }
    }

    [HarmonyPatch(typeof(Building_Grave), "Notify_CorpseBuried")]
    public static class Building_Grave_NotifyCorpseBuriedFuneralHook
    {
        [HarmonyPostfix]
        public static void PlanFuneral(Building_Grave __instance, Pawn worker)
        {
            Pawn planner = (from c in worker.Map.mapPawns.FreeColonistsSpawned
                            where c.relations.OpinionOf(__instance.Corpse.InnerPawn) >= 20
                            select c).RandomElementByWeight((c) => Mathf.Max(0.000001f, c.relations.OpinionOf(__instance.Corpse.InnerPawn) - ((c as PsychologyPawn) != null ? 100f * (1f- (c as PsychologyPawn).psyche.GetPersonalityRating(PersonalityNodeDefOf.Nostalgic)) : 0f)));
            if(planner != null)
            {
                PsychologyPawn realPlanner = planner as PsychologyPawn;
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
                int hour = Enumerable.Range(0, GenDate.HoursPerDay).RandomElementByWeight(h => timeAssignmentFactor(h));
                int date = Find.TickManager.TicksGame + Mathf.RoundToInt(GenDate.TicksPerDay * (2f + (realPlanner != null ? 3f - (5f * realPlanner.psyche.GetPersonalityRating(PersonalityNodeDefOf.Spontaneous)) : 0f)));
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
            }
        }
    }
}
