using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ChildRelationUtility), nameof(ChildRelationUtility.ChanceOfBecomingChildOf))]
    public static class ChildRelationUtility_ChanceOfBecomingChildOf_Patch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void KinseyFactor(ref float __result, Pawn father, Pawn mother, Pawn child)
        {
            /* Kinsey-enabled pawns shouldn't have the Gay trait, so we can just apply the sexuality modifier here. */
            if (father != null && child != null && child.GetFather() == null)
            {
                if (father.GetComp<CompPsychology>() != null && father.GetComp<CompPsychology>().isPsychologyPawn && PsychologyBase.ActivateKinsey())
                {
                    __result *= Mathf.InverseLerp(6f, 0f, father.GetComp<CompPsychology>().Sexuality.kinseyRating);
                }
            }
            if (mother != null && child != null && child.GetMother() == null)
            {
                if (mother.GetComp<CompPsychology>() != null && mother.GetComp<CompPsychology>().isPsychologyPawn && PsychologyBase.ActivateKinsey())
                {
                    __result *= Mathf.InverseLerp(6f, 0f, mother.GetComp<CompPsychology>().Sexuality.kinseyRating);
                }
            }
        }
    }
}
