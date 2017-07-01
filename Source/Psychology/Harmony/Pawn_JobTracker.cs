using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    public static class Pawn_JobTracker_EndCurrentJobPatch
    {
        [HarmonyPrefix]
        public static bool HeavySleeperTrait(Pawn_JobTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            return (__instance.curDriver == null
                || !pawn.RaceProps.Humanlike
                || !__instance.curDriver.asleep
                || Traverse.Create(pawn.needs.rest).Field("lastRestTick").GetValue<int>() < Find.TickManager.TicksGame - 200
                || !pawn.story.traits.HasTrait(TraitDefOfPsychology.HeavySleeper));
        }
    }
}
