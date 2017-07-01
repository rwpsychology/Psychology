using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Need_Rest),"NeedInterval")]
    public static class Need_Rest_IntervalPatch
    {
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

        [HarmonyPostfix]
        public static void MakeInsomniacLessRestful(Need_Rest __instance)
        {
            if (Traverse.Create(__instance).Field("Resting").GetValue<bool>())
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
                {
                    __instance.CurLevel -= 0.005714286f * ((2 * Traverse.Create(__instance).Field("lastRestEffectiveness").GetValue<float>()) / 3);
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
