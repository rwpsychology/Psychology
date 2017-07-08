using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Building_Grave), "Notify_CorpseBuried")]
    public static class Building_Grave_NotifyCorpseBuried_Patch
    {
        [HarmonyPostfix]
        public static void FillGraveThought(Building_Grave __instance, Pawn worker)
        {
            CompArt comp = __instance.GetComp<CompArt>();
            if (worker.needs.mood != null && comp != null)
            {
                worker.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.FilledGraveBleedingHeart);
            }
        }
    }
}
