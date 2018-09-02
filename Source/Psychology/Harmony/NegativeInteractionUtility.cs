using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(NegativeInteractionUtility), nameof(NegativeInteractionUtility.NegativeInteractionChanceFactor))]
    public static class NegativeInteractionUtility_ChancePatch
    {
        [HarmonyPostfix]
        public static void NewFormula(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (PsycheHelper.PsychologyEnabled(initiator))
            {
                SimpleCurve opinionCurve = Traverse.Create(typeof(NegativeInteractionUtility)).Field("CompatibilityFactorCurve").GetValue<SimpleCurve>();
                __result /= opinionCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
                __result *= 2f * PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
            }
        }
    }
}
