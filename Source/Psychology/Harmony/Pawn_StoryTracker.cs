using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn_StoryTracker))]
    [HarmonyPatch(nameof(Pawn_StoryTracker.TitleShort), PropertyMethod.Getter)]
    public static class Pawn_StoryTracker_MayorLabel
    {
        [HarmonyPostfix]
        public static void SetMayorLabel(Pawn_StoryTracker __instance, ref String __result)
        {
            Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (p != null && p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor))
            {
                __result = "mayor";
            }
        }
    }
}
