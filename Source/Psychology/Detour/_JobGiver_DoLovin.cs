using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using UnityEngine;

namespace Psychology.Detour
{
    internal static class _JobGiver_DoLovin
    {
        [DetourMethod(typeof(JobGiver_DoLovin),"TryGiveJob")]
        internal static Job _TryGiveJob(this JobGiver_DoLovin _this, Pawn pawn)
        {
            if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
            {
                return null;
            }
            if (pawn.CurJob == null || !pawn.jobs.curDriver.layingDown || pawn.jobs.curDriver.layingDownBed == null || pawn.jobs.curDriver.layingDownBed.Medical || !pawn.health.capacities.CanBeAwake)
            {
                return null;
            }
            Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
            if (partnerInMyBed == null || !partnerInMyBed.health.capacities.CanBeAwake || Find.TickManager.TicksGame < partnerInMyBed.mindState.canLovinTick)
            {
                return null;
            }
            if (!pawn.CanReserve(partnerInMyBed, 1) || !partnerInMyBed.CanReserve(pawn, 1))
            {
                return null;
            }
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            PsychologyPawn realPartner = partnerInMyBed as PsychologyPawn;
            if(realPawn != null && realPartner != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality != null && realPartner.sexuality != null)
            {
                Rand.PushSeed();
                Rand.Seed = (pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 2) * 60) * 391);
                float random = Rand.Value;
                Rand.PopSeed();
                if (random > realPawn.sexuality.AdjustedSexDrive && random > realPartner.sexuality.AdjustedSexDrive)
                {
                    return null;
                }
            }
            pawn.mindState.awokeVoluntarily = true;
            partnerInMyBed.mindState.awokeVoluntarily = true;
            return new Job(JobDefOf.Lovin, partnerInMyBed, pawn.jobs.curDriver.layingDownBed);
        }
    }
}
