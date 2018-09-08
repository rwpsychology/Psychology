using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(VoluntarilyJoinableLordsStarter), "Tick_TryStartParty")]
    public static class VoluntarilyJoinableLordsStarter_StartPartyPatch
    {
        [LogPerformance]
        [HarmonyPrefix]
        public static bool ExtraSocialiteParties(VoluntarilyJoinableLordsStarter __instance)
        {
            //Transpiler?
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (!map.IsPlayerHome)
            {
                return false;
            }
            int socialiteMod = 1 + map.mapPawns.FreeColonistsSpawned.Where(p => p.story.traits.HasTrait(TraitDefOfPsychology.Socialite)).Count();
            if (Find.TickManager.TicksGame % (GenDate.TicksPerHour * 2) == 0)
            {
                if (Rand.MTBEventOccurs(40f, GenDate.TicksPerDay, (GenDate.TicksPerHour * 2f * (1 + (socialiteMod/(map.mapPawns.ColonistCount > 0 ? map.mapPawns.ColonistCount : 1))))))
                {
                    Traverse.Create(__instance).Field("startPartyASAP").SetValue(true);
                }
                if (Traverse.Create(__instance).Field("startPartyASAP").GetValue<bool>() && Find.TickManager.TicksGame - Traverse.Create(__instance).Field("lastLordStartTick").GetValue<int>() >= (int)(GenDate.TicksPerSeason * 2 / (1 + (socialiteMod / (map.mapPawns.ColonistCount > 0 ? map.mapPawns.ColonistCount : 1)))) && PartyUtility.AcceptableGameConditionsToStartParty(map))
                {
                    Traverse.Create(__instance).Method("TryStartParty", new object[] { }).GetValue();
                }
            }
            return false;
        }
    }
}
