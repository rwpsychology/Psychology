using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Recipe_InstallImplant), "ApplyOnPawn")]
    public static class Recipe_InstallImplant_ApplyPatch
    {
        [HarmonyPrefix]
        public static void BleedingHeartThought(Pawn pawn, Pawn billDoer)
        {
            if (billDoer != null)
                billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
        }
    }
}
