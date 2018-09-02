using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
    public static class Recipe_Surgery_FailPatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(bool __result, Pawn surgeon, Pawn patient)
        {
            if (surgeon.needs.mood != null && __result && patient.Dead)
            {
                surgeon.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.KilledPatientBleedingHeart, patient);
            }
        }
    }
}
