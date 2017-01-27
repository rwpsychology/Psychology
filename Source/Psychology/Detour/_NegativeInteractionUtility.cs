using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _NegativeInteractionUtility
    {
        [DetourMethod(typeof(NegativeInteractionUtility),"NegativeInteractionChanceFactor")]
        public static float NegativeInteractionChanceFactor(Pawn initiator, Pawn recipient)
        {
            PsychologyPawn realInitiator = initiator as PsychologyPawn;
            float num = 1f;
            if (realInitiator == null)
            {
                num = 4f;
                num *= OpinionFactorCurve.Evaluate((float)initiator.relations.OpinionOf(recipient));
                num *= CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
            }
            else
            {
                num *= 2f * realInitiator.psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
                num *= OpinionFactorCurve.Evaluate((float)initiator.relations.OpinionOf(recipient));
            }
            if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
            {
                num *= 2.3f;
            }
            return num;
        }

        private static readonly SimpleCurve CompatibilityFactorCurve = new SimpleCurve
        {
            new CurvePoint(-2.5f, 4f),
            new CurvePoint(-1.5f, 3f),
            new CurvePoint(-0.5f, 2f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1f, 0.75f),
            new CurvePoint(2f, 0.5f),
            new CurvePoint(3f, 0.4f)
        };

        private static readonly SimpleCurve OpinionFactorCurve = new SimpleCurve
        {
            new CurvePoint(-100f, 6f),
            new CurvePoint(-50f, 4f),
            new CurvePoint(-25f, 2f),
            new CurvePoint(0f, 1f),
            new CurvePoint(50f, 0.1f),
            new CurvePoint(100f, 0f)
        };
    }
}
