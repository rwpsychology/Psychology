using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Recipe_InstallNaturalBodyPart), "ApplyOnPawn")]
    public static class Recipe_InstallNaturalBodyPart_ApplyPatch
    {
        [HarmonyPrefix]
        public static void BleedingHeartThought(Pawn pawn, Pawn billDoer)
        {
            if (billDoer != null && billDoer.needs.mood != null)
                billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
        }
    }
}
