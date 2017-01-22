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
            if (pawn.def != otherPawn.def || pawn == otherPawn)
            {
                return 0f;
            }
            Rand.PushSeed();
            Rand.Seed = pawn.HashOffset();
            bool flag = Rand.Value < 0.015f;
            Rand.PopSeed();
            float num = 1f;
            float num2 = 1f;
            float num9 = 1f;
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            PsychologyPawn realPawn = pawn as PsychologyPawn;
            if (PsychologyBase.ActivateKinsey() && realPawn != null)
            {
                flag = true;
                float kinsey = 3 - realPawn.sexuality.kinseyRating;
                float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
                num9 = Mathf.InverseLerp(3f, 0f, kinsey * homo);
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
                num2 = GenMath.FlatHill(0f, 16f, 20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 15f, 0.07f, ageBiologicalYearsFloat2);
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
                {
                    num2 = 1f;
                }
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
                        num = 0f;
                    }
                }
                if ((ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f) && (pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded)))
                {
                    return 0f;
                }
                if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
                {
                    num2 = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.2f;
                }
                else
                {
                    num2 = GenMath.FlatHill(0.2f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.1f, ageBiologicalYearsFloat2);
                }
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
                {
                    num2 = 1f;
                }
            }
            float num3 = 1f;
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
            num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
            {
                num3 = 1f;
            }
            float num4 = 1f;
            foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
            {
                num4 *= current.attractionFactor;
            }
            int num5 = 0;
            if (otherPawn.RaceProps.Humanlike)
            {
                num5 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            }
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) || pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight) == 0f)
            {
                num5 = 0;
            }
            float num6 = 1f;
            if (num5 < 0)
            {
                num6 = 0.3f;
            }
            else if (num5 > 0)
            {
                num6 = 2.3f;
            }
            float num7 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
            float num8 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
            return num * num2 * num3 * num4 * num7 * num8 * num6 * num9;
        }

        [DetourFallback("_SecondaryRomanceChanceFactor")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursSexual = false;
        }
    }
}
