using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;
using Psychology.Detour;

namespace Psychology
{
    internal static class _LovePartnerRelationUtility
    {
        [DetourMethod(typeof(LovePartnerRelationUtility), "LovePartnerRelationGenerationChance")]
        internal static float _LovePartnerRelationGenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        {
            if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if (!PsychologyBase.ActivateKinsey())
            {
                if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
                {
                    return 0f;
                }
                if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    return 0f;
                }
            }
            else if (generated.gender == other.gender && !request.AllowGay)
            {
                return 0f;
            }
            float existingExLoverFactor = 1f;
            if (ex)
            {
                int exLovers = 0;
                List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
                for (int i = 0; i < directRelations.Count; i++)
                {
                    if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
                    {
                        exLovers++;
                    }
                }
                existingExLoverFactor = Mathf.Pow(0.2f, (float)exLovers);
            }
            else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
            {
                return 0f;
            }
            float sexualityFactor = 1f;
            PsychologyPawn realGenerated = generated as PsychologyPawn;
            PsychologyPawn realOther = other as PsychologyPawn;
            if (PsychologyBase.ActivateKinsey() && realGenerated != null && realOther != null)
            {
                float kinsey = 3 - realGenerated.sexuality.kinseyRating;
                float kinsey2 = 3 - realOther.sexuality.kinseyRating;
                float homo = (generated.gender == other.gender) ? 1f : -1f;
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey2 * homo);
            }
            else
            {
                sexualityFactor = (generated.gender != other.gender) ? 1f : 0.01f;
            }
            var GetGenerationChanceAgeFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var GetGenerationChanceAgeGapFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeGapFactor", BindingFlags.Static | BindingFlags.NonPublic);
            float generationChanceAgeFactor = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { generated });
            float generationChanceAgeFactor2 = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { other });
            float generationChanceAgeGapFactor = (float)GetGenerationChanceAgeGapFactor.Invoke(null, new object[] { generated, other, ex });
            float incestFactor = 1f;
            if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                incestFactor = 0.01f;
            }
            float melaninFactor;
            if (request.FixedMelanin.HasValue)
            {
                melaninFactor = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            }
            else
            {
                melaninFactor = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
            }
            return existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
        }
        
        [DetourMethod(typeof(LovePartnerRelationUtility),"ChangeSpouseRelationsToExSpouse")]
        internal static void _ChangeSpouseRelationsToExSpouse(Pawn pawn)
        {
            while (true)
            {
                Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                if (firstDirectRelationPawn == null || (pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && firstDirectRelationPawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)))
                {
                    break;
                }
                pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, firstDirectRelationPawn);
                pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, firstDirectRelationPawn);
            }
        }
    }
}