// Decompiled with JetBrains decompiler
// Type: RimWorld.Recipe_InstallNaturalBodyPart
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Recipe_InstallArtificialBodyPart
    {
        [DetourMethod(typeof(Recipe_InstallArtificialBodyPart), "ApplyOnPawn")]
        internal static void _ApplyOnPawn(this Recipe_InstallArtificialBodyPart r, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
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
            pawn.health.AddHediff(r.recipe.addsHediff, part, null);
        }

        [DetourFallback("_ApplyOnPawn")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursMedical = false;
        }
    }
}
