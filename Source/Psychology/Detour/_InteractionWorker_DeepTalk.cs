using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology
{
    internal static class _InteractionWorker_DeepTalk
    {
        [DetourMethod(typeof(InteractionWorker_DeepTalk),"RandomSelectionWeight")]
        internal static float _RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator is PsychologyPawn || recipient is PsychologyPawn)
            {
                return 0f;
            }
            return BaseSelectionWeight * CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
        }
        
        private const float BaseSelectionWeight = 0.075f;
        
        private static SimpleCurve CompatibilityFactorCurve = new SimpleCurve
        {
            new CurvePoint(-1.5f, 0f),
            new CurvePoint(-0.5f, 0.1f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1f, 1.8f),
            new CurvePoint(2f, 3f)
        };
    }
}
