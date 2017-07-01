using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(NegativeInteractionUtility), "NegativeInteractionChanceFactor")]
    public static class NegativeInteractionUtility_ChancePatch
    {
        [HarmonyPostfix]
        public static void NewFormula(ref float __result, Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            if (realInitiator != null)
            {
                float num = 1f;
                num *= 2f * realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
                num *= Traverse.Create(typeof(NegativeInteractionUtility)).Field("OpinionFactorCurve").GetValue<SimpleCurve>().Evaluate((float)initiator.relations.OpinionOf(recipient));
                if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
                {
                    num *= 2.3f;
                }
                __result = num;
            }
        }
    }
}
