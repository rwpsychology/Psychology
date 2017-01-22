using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallNaturalBodyPart
    {
        [DetourMethod(typeof(Recipe_InstallNaturalBodyPart), "ApplyOnPawn")]
        internal static void _ApplyOnPawn(this Recipe_InstallNaturalBodyPart r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
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
                var MedicalRecipesUtility = Type.GetType("RimWorld.MedicalRecipesUtility, Assembly-CSharp, Version=0.16.6198.16102, Culture=neutral, PublicKeyToken=null");
                var restore = MedicalRecipesUtility.GetMethod("RestorePartAndSpawnAllPreviousParts", BindingFlags.Static | BindingFlags.Public);
                if (restore != null)
                    restore.Invoke(MedicalRecipesUtility, new object[] { pawn, part, billDoer.Position, billDoer.Map });
                else
                    Log.ErrorOnce("Unable to reflect MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts!", 305432421);
                billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
            }
        }

        [DetourFallback("_ApplyOnPawn")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursMedical = false;
        }
    }
}
