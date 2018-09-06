using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
    public static class Need_Rest_IntervalDreamPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void CauseDream(Need_Rest __instance)
        {
            if (Traverse.Create(__instance).Property("Resting").GetValue<bool>())
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (Rand.Value < 0.001f && pawn.RaceProps.Humanlike && !pawn.Awake())
                {
                    if (Rand.Value < 0.5f)
                    {
                        if (Rand.Value < 0.125f)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamNightmare, pawn);
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamBad, pawn);
                        }
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamGood, pawn);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
    public static class Need_Rest_IntervalInsomniacPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void MakeInsomniacLessRestful(Need_Rest __instance)
        {
            if (Traverse.Create(__instance).Property("Resting").GetValue<bool>())
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (!Traverse.Create(__instance).Property("IsFrozen").GetValue<bool>() && pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac) && !pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.SleepingPills))
                {
                    __instance.CurLevel -= (2f * 150f * Need_Rest.BaseRestGainPerTick) / 3f;
                    if (__instance.CurLevel > (Need_Rest.DefaultNaturalWakeThreshold / 4f))
                    {
                        if (Rand.MTBEventOccurs((Need_Rest.DefaultNaturalWakeThreshold - __instance.CurLevel) / 4f, GenDate.TicksPerDay, 150f) && !pawn.Awake())
                        {
                            pawn.jobs.curDriver.asleep = false;
                            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        }
                    }
                }
            }
        }
    }
}
