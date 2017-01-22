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
    // Token: 0x0200031F RID: 799
    internal static class _ChildRelationUtility
    {
        // Token: 0x06000C59 RID: 3161 RVA: 0x0003C700 File Offset: 0x0003A900
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
			float num = 1f;
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 1f;
            if (father != null && child.GetFather() == null)
            {
                PsychologyPawn realFather = father as PsychologyPawn;
                num = (float)GetParentAgeFactor.Invoke(typeof(ChildRelationUtility), new object[] { father, child, 14f, 30f, 50f });
				if (num == 0f)
				{
					return 0f;
                }
                if (PsychologyBase.ActivateKinsey() && realFather != null)
                {
                    num4 = Mathf.InverseLerp(6f, 0f, realFather.sexuality.kinseyRating);
                }
                else if (father.story.traits.HasTrait(TraitDefOf.Gay))
				{
					num4 = 0.1f;
				}
            }
            if (mother != null && child.GetMother() == null)
            {
                PsychologyPawn realMother = mother as PsychologyPawn;
                num2 = (float)GetParentAgeFactor.Invoke(typeof(ChildRelationUtility), new object[] { mother, child, 16f, 27f, 45f });
				if (num2 == 0f)
				{
					return 0f;
                }
                int num5 = (int)NumberOfChildrenFemaleWantsEver.Invoke(typeof(ChildRelationUtility), new object[] { mother });
				if (mother.relations.ChildrenCount >= num5)
				{
					return 0f;
				}
				num3 = 1f - (float)mother.relations.ChildrenCount / (float)num5;
                if (PsychologyBase.ActivateKinsey() && realMother != null)
                {
                    num4 = Mathf.InverseLerp(6f, 0f, realMother.sexuality.kinseyRating);
                }
                else if (mother.story.traits.HasTrait(TraitDefOf.Gay))
				{
					num4 = 0.1f;
				}
            }
            float num6 = 1f;
			if (mother != null)
			{
				Pawn firstDirectRelationPawn = mother.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn != null && firstDirectRelationPawn != father)
				{
					num6 *= 0.15f;
				}
			}
			if (father != null)
			{
				Pawn firstDirectRelationPawn2 = father.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != mother)
				{
					num6 *= 0.15f;
				}
			}
			return skinColorFactor * num * num2 * num3 * num6 * num4;
        }
    }
}
