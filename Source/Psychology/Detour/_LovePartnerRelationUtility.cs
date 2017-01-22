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
    // Token: 0x0200032A RID: 810
    internal static class _LovePartnerRelationUtility
    {
        // Token: 0x06000C9A RID: 3226 RVA: 0x0003EAD8 File Offset: 0x0003CCD8
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
            float num = 1f;
            if (ex)
            {
                int num2 = 0;
                List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
                for (int i = 0; i < directRelations.Count; i++)
                {
                    if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
                    {
                        num2++;
                    }
                }
                num = Mathf.Pow(0.2f, (float)num2);
            }
            else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
            {
                return 0f;
            }
            float num3 = 1f;
            PsychologyPawn realGenerated = generated as PsychologyPawn;
            PsychologyPawn realOther = other as PsychologyPawn;
            if (PsychologyBase.ActivateKinsey() && realGenerated != null && realOther != null)
            {
                float kinsey = 3 - realGenerated.sexuality.kinseyRating;
                float kinsey2 = 3 - realOther.sexuality.kinseyRating;
                float homo = (generated.gender == other.gender) ? 1f : -1f;
                num3 *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
                num3 *= Mathf.InverseLerp(3f, 0f, kinsey2 * homo);
            }
            else
            {
                num3 = (generated.gender != other.gender) ? 1f : 0.01f;
            }
            var GetGenerationChanceAgeFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var GetGenerationChanceAgeGapFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeGapFactor", BindingFlags.Static | BindingFlags.NonPublic);
            float generationChanceAgeFactor = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { generated });
            float generationChanceAgeFactor2 = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { other });
            float generationChanceAgeGapFactor = (float)GetGenerationChanceAgeGapFactor.Invoke(null, new object[] { generated, other, ex });
            float num4 = 1f;
            if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                num4 = 0.01f;
            }
            float num5;
            if (request.FixedMelanin.HasValue)
            {
                num5 = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            }
            else
            {
                num5 = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
            }
            return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
        }

        // Token: 0x06000E62 RID: 3682 RVA: 0x0004A000 File Offset: 0x00048200
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