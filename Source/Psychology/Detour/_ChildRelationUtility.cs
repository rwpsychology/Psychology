using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;
using HugsLib;

namespace Psychology.Detour
{
    internal static class _ChildRelationUtility
    {
        [DetourMethod(typeof(ChildRelationUtility),"ChanceOfBecomingChildOf")]
        internal static float _ChanceOfBecomingChildOf(Pawn child, Pawn father, Pawn mother, PawnGenerationRequest? childGenerationRequest, PawnGenerationRequest? fatherGenerationRequest, PawnGenerationRequest? motherGenerationRequest)
        {
            var GetMelanin = typeof(ChildRelationUtility).GetMethod("GetMelanin", BindingFlags.Static | BindingFlags.NonPublic);
            var GetSkinColorFactor = typeof(ChildRelationUtility).GetMethod("GetSkinColorFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var GetParentAgeFactor = typeof(ChildRelationUtility).GetMethod("GetParentAgeFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var NumberOfChildrenFemaleWantsEver = typeof(ChildRelationUtility).GetMethod("NumberOfChildrenFemaleWantsEver", BindingFlags.Static | BindingFlags.NonPublic);
            if (GetMelanin == null)
            {
                Log.Error("Could not reflect ChildRelationUtility.GetMelanin!");
                return 0f;
            }
            if (GetSkinColorFactor == null)
            {
                Log.Error("Could not reflect ChildRelationUtility.GetSkinColorFactor!");
                return 0f;
            }
            if (GetParentAgeFactor == null)
            {
                Log.Error("Could not reflect ChildRelationUtility.GetParentAgeFactor!");
                return 0f;
            }
            if (NumberOfChildrenFemaleWantsEver == null)
            {
                Log.Error("Could not reflect ChildRelationUtility.NumberOfChildrenFemaleWantsEver!");
                return 0f;
            }
            if (father != null && father.gender != Gender.Male)
			{
				Log.Warning("Tried to calculate chance for father with gender \"" + father.gender + "\".");
				return 0f;
			}
			if (mother != null && mother.gender != Gender.Female)
			{
				Log.Warning("Tried to calculate chance for mother with gender \"" + mother.gender + "\".");
				return 0f;
			}
			if (father != null && child.GetFather() != null && child.GetFather() != father)
			{
				return 0f;
			}
			if (mother != null && child.GetMother() != null && child.GetMother() != mother)
			{
				return 0f;
			}
			if (mother != null && father != null && !LovePartnerRelationUtility.LovePartnerRelationExists(mother, father) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(mother, father))
			{
				return 0f;
			}
			float? melanin = (float?)GetMelanin.Invoke(typeof(ChildRelationUtility), new object[] { child, childGenerationRequest });
            float? melanin2 = (float?)GetMelanin.Invoke(typeof(ChildRelationUtility), new object[] { father, fatherGenerationRequest });
            float? melanin3 = (float?)GetMelanin.Invoke(typeof(ChildRelationUtility), new object[] { mother, motherGenerationRequest });
            bool fatherIsNew = father != null && child.GetFather() != father;
			bool motherIsNew = mother != null && child.GetMother() != mother;
			float skinColorFactor = (float)GetSkinColorFactor.Invoke(typeof(ChildRelationUtility), new object[] { melanin, melanin2, melanin3, fatherIsNew, motherIsNew });
            if (skinColorFactor <= 0f)
			{
				return 0f;
			}
			float maleAgeFactor = 1f;
			float femaleAgeFactor = 1f;
			float existingChildrenFactor = 1f;
			float maleSexualityFactor = 1f;
            float femaleSexualityFactor = 1f;
            if (father != null && child.GetFather() == null)
            {
                PsychologyPawn realFather = father as PsychologyPawn;
                maleAgeFactor = (float)GetParentAgeFactor.Invoke(typeof(ChildRelationUtility), new object[] { father, child, 14f, 30f, 50f });
				if (maleAgeFactor == 0f)
				{
					return 0f;
                }
                if (PsychologyBase.ActivateKinsey() && realFather != null)
                {
                    maleSexualityFactor = Mathf.InverseLerp(6f, 0f, realFather.sexuality.kinseyRating);
                }
                else if (father.story.traits.HasTrait(TraitDefOf.Gay))
				{
                    maleSexualityFactor = 0.1f;
				}
            }
            if (mother != null && child.GetMother() == null)
            {
                PsychologyPawn realMother = mother as PsychologyPawn;
                femaleAgeFactor = (float)GetParentAgeFactor.Invoke(typeof(ChildRelationUtility), new object[] { mother, child, 16f, 27f, 45f });
				if (femaleAgeFactor == 0f)
				{
					return 0f;
                }
                int desiredChildrenFactor = (int)NumberOfChildrenFemaleWantsEver.Invoke(typeof(ChildRelationUtility), new object[] { mother });
				if (mother.relations.ChildrenCount >= desiredChildrenFactor)
				{
					return 0f;
				}
				existingChildrenFactor = 1f - (float)mother.relations.ChildrenCount / (float)desiredChildrenFactor;
                if (PsychologyBase.ActivateKinsey() && realMother != null)
                {
                    femaleSexualityFactor = Mathf.InverseLerp(6f, 0f, realMother.sexuality.kinseyRating);
                }
                else if (mother.story.traits.HasTrait(TraitDefOf.Gay))
				{
					femaleSexualityFactor = 0.1f;
				}
            }
            float relationshipFactor = 1f;
			if (mother != null)
			{
				Pawn firstDirectRelationPawn = mother.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn != null && firstDirectRelationPawn != father)
				{
					relationshipFactor *= 0.15f;
				}
			}
			if (father != null)
			{
				Pawn firstDirectRelationPawn2 = father.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != mother)
				{
					relationshipFactor *= 0.15f;
				}
			}
			return skinColorFactor * maleAgeFactor * maleSexualityFactor * femaleSexualityFactor * femaleAgeFactor * existingChildrenFactor * relationshipFactor;
        }
    }
}
