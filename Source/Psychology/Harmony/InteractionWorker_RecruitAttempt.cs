using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "DoRecruit")]
    public static class InteractionWorker_RecruitAttempt_DoRecruitPatch
    {
        [HarmonyPostfix]
        public static void AddCapturedThoughts(Pawn recruiter, Pawn recruitee)
        {
            if (recruitee.RaceProps.Humanlike)
            {
                recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
                recruitee.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.RapportBuilt);
                List<Pawn> allFactionPawns = Find.Maps.SelectMany(m => from p in m.mapPawns.FreeColonistsSpawned
                                                                       where p != recruitee
                                                                       select p).ToList<Pawn>();
                foreach (Pawn pawn in allFactionPawns)
                {
                    recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CapturedMe, pawn);
                }
            }
        }
    }
}
