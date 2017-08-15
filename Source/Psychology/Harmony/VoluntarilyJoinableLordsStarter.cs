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
        [HarmonyPrefix]
        public static bool ExtraSocialiteParties(VoluntarilyJoinableLordsStarter __instance)
        {
            //Transpiler?
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (map.IsPlayerHome)
            {
                return false;
            }
            int socialiteMod = 1;
            IEnumerable<Pawn> allPawnsSpawned = map.mapPawns.FreeColonistsSpawned;
            foreach (Pawn pawn in allPawnsSpawned)
            {
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Socialite))
                {
                    socialiteMod++;
                }
            }
            if (Find.TickManager.TicksGame % GenDate.TicksPerHour * 2 == 0)
            {
                if (Rand.MTBEventOccurs(40f, GenDate.TicksPerDay, (GenDate.TicksPerHour * 2f * socialiteMod)))
                {
                    Traverse.Create(__instance).Field("startPartyASAP").SetValue(true);
                }
                if (Traverse.Create(__instance).Field("startPartyASAP").GetValue<bool>() && Find.TickManager.TicksGame - Traverse.Create(__instance).Field("lastLordStartTick").GetValue<int>() >= (int)(GenDate.TicksPerSeason * 2 / socialiteMod) && PartyUtility.AcceptableGameConditionsToStartParty(map))
                {
                    Traverse.Create(__instance).Method("TryStartParty", new object[] { }).GetValue();
                }
            }
            return false;
        }
    }
}
