using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Pawn_RelationsTracker
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this Pawn_RelationsTracker _this)
        {
            if (_Pawn_RelationsTracker._pawn == null)
            {
                _Pawn_RelationsTracker._pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_Pawn_RelationsTracker._pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 305432421);
                }
            }
            return (Pawn)_Pawn_RelationsTracker._pawn.GetValue(_this);
        }

        [DetourMethod(typeof(Pawn_RelationsTracker),"Notify_RescuedBy")]
        internal static void _Notify_RescuedBy(this Pawn_RelationsTracker t, Pawn rescuer)
        {

            if (rescuer.RaceProps.Humanlike && t.canGetRescuedThought)
            {
                t.GetPawn().needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RescuedMe, rescuer);
                t.canGetRescuedThought = false;
                rescuer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.RescuedBleedingHeart, t.GetPawn());
            }
        }

        [DetourMethod(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
        internal static float _SecondaryRomanceChanceFactor(this Pawn_RelationsTracker t, Pawn otherPawn)
        {
            Pawn pawn = t.GetPawn();
            if (!otherPawn.RaceProps.Humanlike || pawn == otherPawn)
            {
                return 0f;
            }
            Rand.PushSeed();
            Rand.Seed = pawn.HashOffset();
            bool flag = Rand.Value < 0.015f;
            Rand.PopSeed();
            float ageFactor = 1f;
            float sexualityFactor = 1f;
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if (PsychologyBase.ActivateKinsey() && realPawn != null && realPawn.sexuality != null)
            {
                flag = true;
                float kinsey = 3 - realPawn.sexuality.kinseyRating;
                float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
                sexualityFactor = Mathf.InverseLerp(3f, 0f, kinsey * homo);
            }
            if (pawn.gender == Gender.Male)
            {
                if (!flag)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender == Gender.Female)
                        {
                            return 0f;
                        }
                    }
                    else if (otherPawn.gender == Gender.Male)
                    {
                        return 0f;
                    }
                }
                ageFactor = GenMath.FlatHill(0f, 16f, 20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 15f, 0.07f, ageBiologicalYearsFloat2);
            }
            else if (pawn.gender == Gender.Female)
            {
                if (!flag)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender == Gender.Male)
                        {
                            return 0f;
                        }
                    }
                    else if (otherPawn.gender == Gender.Female)
                    {
                        return 0f;
                    }
                }
                if ((ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f) && (!pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded)))
                {
                    return 0f;
                }
                if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
                {
                    ageFactor = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.2f;
                }
                else
                {
                    ageFactor = GenMath.FlatHill(0.2f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.1f, ageBiologicalYearsFloat2);
                }
            }
            float disabilityFactor = 1f;
            disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
            disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
            disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
            if (realPawn != null)
            {
                disabilityFactor = Mathf.Lerp(ageFactor, 1f+ageFactor, realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
                ageFactor = Mathf.Lerp(ageFactor, 1f+ageFactor, realPawn.psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
            }
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
            {
                ageFactor = 1f;
                disabilityFactor = 1f;
            }
            float relationFactor = 1f;
            foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
            {
                relationFactor *= current.attractionFactor;
            }
            int beauty = 0;
            if (otherPawn.RaceProps.Humanlike)
            {
                beauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            }
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) || pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight) == 0f)
            {
                beauty = 0;
            }
            float beautyFactor = 1f;
            if (beauty < 0)
            {
                beautyFactor = 0.3f;
            }
            else if (beauty > 0)
            {
                beautyFactor = 2.3f;
            }
            if (realPawn != null && PsychologyBase.ActivateKinsey() && realPawn.sexuality != null && realPawn.sexuality.AdjustedSexDrive < 1f)
            {
                beautyFactor = Mathf.Pow(beautyFactor, realPawn.sexuality.AdjustedSexDrive);
                ageFactor = Mathf.Pow(ageFactor, realPawn.sexuality.AdjustedSexDrive);
                disabilityFactor = Mathf.Pow(disabilityFactor, realPawn.sexuality.AdjustedSexDrive);
            }
            float initiatorYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
            float recipientYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
            return 1f * sexualityFactor * ageFactor * disabilityFactor * relationFactor * beautyFactor * initiatorYouthFactor * recipientYouthFactor;
        }

        [DetourFallback("_SecondaryRomanceChanceFactor")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursSexual = false;
        }

        [DetourFallback("_Notify_RescuedBy")]
        public static void DetourFallbackHandlerRescuedBy(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            Log.Warning("[Psychology] Unable to detour Notify_RescuedBy. This is intentional if you've loaded Hospitality before Psychology.");
        }
    }
}
