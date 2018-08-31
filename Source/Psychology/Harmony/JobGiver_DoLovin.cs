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
            Pawn partner = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
            if (PsycheHelper.PsychologyEnabled(pawn) && PsycheHelper.PsychologyEnabled(partner) && PsychologyBase.ActivateKinsey())
            {
                float random = Rand.ValueSeeded((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 2) * 60) * 391));
                float random2 = Rand.ValueSeeded((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(partner) + GenLocalDate.Year(partner) + (int)(GenLocalDate.DayPercent(partner) * 2) * 60) * 391));
                if (random > PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive && random2 > PsycheHelper.Comp(partner).Sexuality.AdjustedSexDrive)
                {
                    __result = null;
                }
            }
        }
    }
}
