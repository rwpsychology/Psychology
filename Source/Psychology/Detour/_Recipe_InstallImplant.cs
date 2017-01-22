using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallImplant
    {
        [DetourMethod(typeof(Recipe_InstallImplant), "ApplyOnPawn")]
        internal static void _ApplyOnPawn(this Recipe_InstallImplant r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {
                var CheckSurgeryFail = typeof(Recipe_Surgery).GetMethod("CheckSurgeryFail", BindingFlags.Instance | BindingFlags.NonPublic);
                if ((bool)CheckSurgeryFail.Invoke(r, new object[] { billDoer, pawn, ingredients, part }))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
            }
            pawn.health.AddHediff(r.recipe.addsHediff, part, null);
        }

        [DetourFallback("_ApplyOnPawn")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursMedical = false;
        }
    }
}
