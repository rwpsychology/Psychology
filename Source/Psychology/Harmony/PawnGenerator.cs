using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;
using System.Reflection;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    public static class PawnGenerator_GenerateTraitsPatch
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool KinseyException(ref Pawn pawn, PawnGenerationRequest request)
        {
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                if (PsychologyBase.ActivateKinsey())
                {
                    while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating > 2 && !request.AllowGay)
                    {
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                    }
                    if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
                    {
                        while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating < 2)
                        {
                            PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                        }
                    }
                    else if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                    {
                        while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating > 4)
                        {
                            PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                        }
                    }
                }
            }
            return true;
        }
    }
}
