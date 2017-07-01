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
            PsychologyPawn newPawn = pawn as PsychologyPawn;
            if (newPawn != null)
            {
                newPawn.psyche.Initialize();
                if (PsychologyBase.ActivateKinsey())
                {
                    newPawn.sexuality.GenerateSexuality();
                    while (newPawn.sexuality.kinseyRating > 2 && !request.AllowGay)
                    {
                        newPawn.sexuality.GenerateSexuality();
                    }
                    if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
                    {
                        while (newPawn.sexuality.kinseyRating < 2)
                        {
                            newPawn.sexuality.GenerateSexuality();
                        }
                    }
                    else if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                    {
                        while (newPawn.sexuality.kinseyRating > 4)
                        {
                            newPawn.sexuality.GenerateSexuality();
                        }
                    }
                    pawn = newPawn;
                }
            }
            return true;
        }
    }
}
