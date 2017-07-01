using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(JobGiver_DoLovin), "TryGiveJob")]
    public static class JobGiver_DoLovin_JobPatch
    {
        [HarmonyPostfix]
        public static void CancelJob(ref Job __result, Pawn pawn)
        {
            Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            PsychologyPawn realPartner = partnerInMyBed as PsychologyPawn;
            if (realPawn != null && realPartner != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality != null && realPartner.sexuality != null)
            {
                float random = Rand.ValueSeeded((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 2) * 60) * 391));
                if (random > realPawn.sexuality.AdjustedSexDrive && random > realPartner.sexuality.AdjustedSexDrive)
                {
                    __result = null;
                }
            }
        }
    }
}
