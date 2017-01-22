using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    // Token: 0x02000341 RID: 833
    internal static class _PawnRelationWorker_Sibling
    {
        // Token: 0x06000CF2 RID: 3314 RVA: 0x00040A6C File Offset: 0x0003EC6C
        [DetourMethod(typeof(PawnRelationWorker_Sibling), "GenerateParent")]
        internal static Pawn _GenerateParent(Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
        {
            float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
            float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
            float num = (genderToGenerate != Gender.Male) ? 16f : 14f;
            float num2 = (genderToGenerate != Gender.Male) ? 45f : 50f;
            float num3 = (genderToGenerate != Gender.Male) ? 27f : 30f;
            float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
            float maxChronologicalAge = num4 + (num2 - num);
            float midChronologicalAge = num4 + (num3 - num);
            float value;
            float value2;
            float value3;
            string last;
            var _GenerateParentParams = typeof(PawnRelationWorker_Sibling).GetMethod("GenerateParentParams", BindingFlags.Static | BindingFlags.NonPublic);
            var parameters = new object[]{num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, null, null, null, null};
            _GenerateParentParams.Invoke(null, parameters);
            value = (float)parameters[7];
            value2 = (float)parameters[8];
            value3 = (float)parameters[9];
            last = (string)parameters[10];
            bool allowGay = true;
            float num5 = 1f;
            if (PsychologyBase.ActivateKinsey())
            {
                PsychologyPawn parent = null;
                if (genderToGenerate == Gender.Male && existingChild.GetMother() != null)
                {
                    parent = existingChild.GetMother() as PsychologyPawn;
                }
                else if (genderToGenerate == Gender.Female && existingChild.GetFather() != null)
                {
                    parent = existingChild.GetFather() as PsychologyPawn;
                }
                if (parent != null)
                {
                    float kinsey = 3 - parent.sexuality.kinseyRating;
                    num5 = Mathf.InverseLerp(3f, 0f, -kinsey);
                    if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < num5)
                    {
                        last = ((NameTriple)parent.Name).Last;
                        allowGay = false;
                    }
                }
            }
            else if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < 0.8f)
            {
                if (genderToGenerate == Gender.Male && existingChild.GetMother() != null && !existingChild.GetMother().story.traits.HasTrait(TraitDefOf.Gay))
                {
                    last = ((NameTriple)existingChild.GetMother().Name).Last;
                    allowGay = false;
                }
                else if (genderToGenerate == Gender.Female && existingChild.GetFather() != null && !existingChild.GetFather().story.traits.HasTrait(TraitDefOf.Gay))
                {
                    last = ((NameTriple)existingChild.GetFather().Name).Last;
                    allowGay = false;
                }
            }
            Faction faction = existingChild.Faction;
            if (faction == null || faction.IsPlayer)
            {
                bool tryMedievalOrBetter = faction != null && faction.def.techLevel >= TechLevel.Medieval;
                Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, true);
            }
            Gender? fixedGender = new Gender?(genderToGenerate);
            float? fixedMelanin = new float?(value3);
            string fixedLastName = last;
            PawnGenerationRequest request = new PawnGenerationRequest(existingChild.kindDef, faction, PawnGenerationContext.NonPlayer, null, true, false, true, true, false, false, 1f, false, allowGay, true, null, new float?(value), new float?(value2), fixedGender, fixedMelanin, fixedLastName);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (!Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
            }
            return pawn;
        }
    }
}
