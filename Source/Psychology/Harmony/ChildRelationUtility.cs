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
    [HarmonyPatch(typeof(ChildRelationUtility), "ChanceOfBecomingChildOf")]
    public static class ChildRelationUtility_ChanceOfBecomingChildOf_Patch
    {
        [HarmonyPostfix]
        public static void KinseyFactor(ref float __result, Pawn father, Pawn mother, Pawn child)
        {
            /* Kinsey-enabled pawns shouldn't have the Gay trait, so we can just apply the sexuality modifier here. */
            if (father != null && child != null && child.GetFather() == null)
            {
                PsychologyPawn realFather = father as PsychologyPawn;
                if (PsychologyBase.ActivateKinsey() && realFather != null)
                {
                    __result *= Mathf.InverseLerp(6f, 0f, realFather.sexuality.kinseyRating);
                }
            }
            if (mother != null && child != null && child.GetMother() == null)
            {
                PsychologyPawn realMother = mother as PsychologyPawn;
                if (PsychologyBase.ActivateKinsey() && realMother != null)
                {
                    __result *= Mathf.InverseLerp(6f, 0f, realMother.sexuality.kinseyRating);
                }
            }
        }
    }
}
