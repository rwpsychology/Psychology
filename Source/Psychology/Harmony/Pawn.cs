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

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn), "CheckAcceptArrest")]
    public static class Pawn_ArrestPatch
    {
        [HarmonyPostfix]
        public static void PsychologyArrestChance(Pawn __instance, ref bool __result, Pawn arrester)
        {
            if (__result && !__instance.health.Downed && (__instance.story == null || !__instance.story.WorkTagIsDisabled(WorkTags.Violent)))
            {
                __result = false;
            }
            if (Rand.Value < (arrester.GetStatValue(StatDefOfPsychology.ArrestPeacefullyChance) * (Mathf.InverseLerp(-100f, 100f, __instance.relations.OpinionOf(arrester))) * (arrester.Faction == __instance.Faction ? 1.5 : 1)))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "PreTraded")]
    public static class Pawn_PreTradedPatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(Pawn __instance, TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            if (action == TradeAction.PlayerSells)
            {
                if (__instance.RaceProps.Humanlike)
                {
                    foreach (Pawn current in from x in PawnsFinder.AllMapsCaravansAndTravelingTransportPods
                                             where x.IsColonist || x.IsPrisonerOfColony
                                             select x)
                    {
                        current.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.KnowPrisonerSoldBleedingHeart, null);
                    }
                }
            }
        }
    }
}
