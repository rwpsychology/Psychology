using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnRelationWorker_Sibling), "GenerateParent")]
    public static class PawnRelationWorker_Sibling_GenerateParentPatch
    {
        [HarmonyPrefix]
        public static bool KinseyException(ref Pawn __result, Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
        {
            if (PsychologyBase.ActivateKinsey())
            {
                //TODO: Turn this into a transpiler instead of a prefix.
                float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
                float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
                float num = (genderToGenerate != Gender.Male) ? 16f : 14f;
                float num2 = (genderToGenerate != Gender.Male) ? 45f : 50f;
                float num3 = (genderToGenerate != Gender.Male) ? 27f : 30f;
                float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
                float maxChronologicalAge = num4 + (num2 - num);
                float midChronologicalAge = num4 + (num3 - num);
                var parameters = new object[] { num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, null, null, null, null };
                Traverse.Create(typeof(PawnRelationWorker_Sibling)).Method("GenerateParentParams", new Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Pawn), typeof(Pawn), typeof(PawnGenerationRequest), typeof(float).MakeByRefType(), typeof(float).MakeByRefType(), typeof(float).MakeByRefType(), typeof(string).MakeByRefType() }).GetValue(parameters);
                float value = (float)parameters[7];
                float value2 = (float)parameters[8];
                float value3 = (float)parameters[9];
                string last = (string)parameters[10];
                bool allowGay = true;
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
                    float num5 = Mathf.InverseLerp(3f, 0f, -kinsey);
                    if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < num5)
                    {
                        last = ((NameTriple)parent.Name).Last;
                        allowGay = false;
                    }
                }
                Faction faction = existingChild.Faction;
                if (faction == null || faction.IsPlayer)
                {
                    bool tryMedievalOrBetter = faction != null && faction.def.techLevel >= TechLevel.Medieval;
                    if (!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, true, TechLevel.Undefined))
                    {
                        faction = Faction.OfSpacer;
                    }
                }
                PawnKindDef kindDef = existingChild.kindDef;
                Faction faction2 = faction;
                bool forceGenerateNewPawn = true;
                bool allowDead = true;
                bool allowDowned = true;
                bool canGeneratePawnRelations = false;
                Gender? fixedGender = new Gender?(genderToGenerate);
                float? fixedMelanin = new float?(value3);
                string fixedLastName = last;
                PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction2, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn, false, allowDead, allowDowned, canGeneratePawnRelations, false, 1f, false, allowGay, true, false, false, false, false, null, null, new float?(value), new float?(value2), fixedGender, fixedMelanin, fixedLastName);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (!Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
                }
                __result = pawn;
                return false;
            }
            return true;
        }
    }
}
