using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(LovePartnerRelationUtility),"LovePartnerRelationGenerationChance")]
    public static class LovePartnerRelationUtility_GenerationChancePatch
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void PsychologyFormula(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        {
            /* Throw away the existing result and substitute our own formula. */
            float sexualityFactor = 1f;
            if (PsycheHelper.PsychologyEnabled(generated) && PsycheHelper.PsychologyEnabled(other) && PsychologyBase.ActivateKinsey())
            {
                float kinsey = 3 - PsycheHelper.Comp(generated).Sexuality.kinseyRating;
                float kinsey2 = 3 - PsycheHelper.Comp(other).Sexuality.kinseyRating;
                float homo = (generated.gender == other.gender) ? 1f : -1f;
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey2 * homo);
            }
            else
            {
                sexualityFactor = (generated.gender != other.gender) ? 1f : 0.01f;
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
                __result = 0f;
                return;
            }
            float generationChanceAgeFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { generated });
            float generationChanceAgeFactor2 = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { other });
            float generationChanceAgeGapFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeGapFactor", new[] { typeof(Pawn), typeof(Pawn), typeof(bool) }).GetValue<float>(new object[] { generated, other, ex });
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
            __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility), "ChangeSpouseRelationsToExSpouse")]
    public static class LovePartnerRelationUtility_PolygamousSpousePatch
    {
        [HarmonyPrefix]
        internal static bool PolygamousException(Pawn pawn)
        {
            if(pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
            {
                IEnumerable<Pawn> spouses = (from p in pawn.relations.RelatedPawns
                                             where pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, p)
                                             select p);
                foreach (Pawn spousePawn in spouses)
                {
                    if (!spousePawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                    {
                        pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, spousePawn);
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, spousePawn);
                    }
                }
                return false;
            }
            return true;
        }
    }
}