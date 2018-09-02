using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public static class TendUtility_TendPatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(Pawn doctor, Pawn patient)
        {
            if (doctor != null)
            {
                doctor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DoctorBleedingHeart, patient);
            }
        }
    }
}
