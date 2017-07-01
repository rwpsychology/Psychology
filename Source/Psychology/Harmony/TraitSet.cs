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
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class TraitSet_GainTraitPatch
    {
        [HarmonyPrefix]
        public static bool KinseyException(ref TraitSet __instance, Trait trait)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if (PsychologyBase.ActivateKinsey() && trait.def == TraitDefOf.Gay)
            {
                return false;
            }
            if (realPawn != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality.romanticDrive < 0.5f)
            {
                if (trait.def == TraitDefOfPsychology.Codependent)
                {
                    return false;
                }
            }
            if (realPawn != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality.sexDrive < 0.5f)
            {
                if (trait.def == TraitDefOfPsychology.Lecher)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
