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
    [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
    public static class TraitSet_GainTraitPatch
    {
        [HarmonyPrefix]
        public static bool KinseyException(ref TraitSet __instance, Trait trait)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Pawn_SexualityTracker ps = null;
            if (pawn.GetComp<CompPsychology>() != null && pawn.GetComp<CompPsychology>().isPsychologyPawn)
            {
                ps = pawn.GetComp<CompPsychology>().Sexuality;
            }
            if (ps != null && PsychologyBase.ActivateKinsey() && trait.def == TraitDefOf.Gay)
            {
                return false;
            }
            if (ps != null && PsychologyBase.ActivateKinsey() && ps.romanticDrive < 0.5f)
            {
                if (trait.def == TraitDefOfPsychology.Codependent)
                {
                    return false;
                }
            }
            if (ps != null && PsychologyBase.ActivateKinsey() && ps.sexDrive < 0.5f)
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
