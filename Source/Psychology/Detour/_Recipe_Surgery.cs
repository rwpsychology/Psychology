using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Recipe_Surgery
    {
        [DetourMethod(typeof(Recipe_Surgery), "CheckSurgeryFail")]
        internal static bool _CheckSurgeryFail(this Recipe_Surgery s, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part)
        {
            float successChance = 1f;
            float surgeonSkillFactor = surgeon.GetStatValue(StatDefOf.SurgerySuccessChance, true);
            if (surgeonSkillFactor < 1f)
            {
                surgeonSkillFactor = Mathf.Pow(surgeonSkillFactor, s.recipe.surgeonSurgerySuccessChanceExponent);
            }
            successChance *= surgeonSkillFactor;
            Room room = surgeon.GetRoom();
            if (room != null)
            {
                float roomFactor = room.GetStat(RoomStatDefOf.SurgerySuccessChanceFactor);
                if (roomFactor < 1f)
                {
                    roomFactor = Mathf.Pow(roomFactor, s.recipe.roomSurgerySuccessChanceFactorExponent);
                }
                successChance *= roomFactor;
            }
            var GetAverageMedicalPotency = typeof(Recipe_Surgery).GetMethod("GetAverageMedicalPotency", BindingFlags.Instance | BindingFlags.NonPublic);
            if (GetAverageMedicalPotency != null)
                successChance *= (float)GetAverageMedicalPotency.Invoke(s, new object[] { ingredients });
            successChance *= s.recipe.surgerySuccessChanceFactor;
            if (Rand.Value > successChance)
            {
                if (Rand.Value < s.recipe.deathOnFailedSurgeryChance)
                {
                    int num4 = 0;
                    while (!patient.Dead)
                    {
                        HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
                        num4++;
                        if (num4 > 300)
                        {
                            Log.Error("Could not kill patient.");
                            break;
                        }
                    }
                }
                else if (Rand.Value < 0.5f)
                {
                    if (Rand.Value < 0.1f)
                    {
                        Messages.Message("MessageMedicalOperationFailureRidiculous".Translate(new object[]
                        {
                            surgeon.LabelShort,
                            patient.LabelShort
                        }), patient, MessageSound.SeriousAlert);
                        HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
                    }
                    else
                    {
                        Messages.Message("MessageMedicalOperationFailureCatastrophic".Translate(new object[]
                        {
                            surgeon.LabelShort,
                            patient.LabelShort
                        }), patient, MessageSound.SeriousAlert);
                        HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
                    }
                }
                else
                {
                    Messages.Message("MessageMedicalOperationFailureMinor".Translate(new object[]
                    {
                        surgeon.LabelShort,
                        patient.LabelShort
                    }), patient, MessageSound.Negative);
                    HealthUtility.GiveInjuriesOperationFailureMinor(patient, part);
                }
                if (!patient.Dead)
                {
                    var TryGainBotchedSurgeryThought = typeof(Recipe_Surgery).GetMethod("TryGainBotchedSurgeryThought", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (TryGainBotchedSurgeryThought != null)
                        TryGainBotchedSurgeryThought.Invoke(s, new object[] { patient, surgeon });
                    else
                        Log.ErrorOnce("Unable to reflect Recipe_MedicalOperation.TryGainBotchedSurgeryThought!", 305432421);
                }
                else
                {
                    surgeon.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KilledPatientBleedingHeart, patient);
                }
                return true;
            }
            return false;
        }

        [DetourFallback("_CheckSurgeryFail")]
        public static void DetourFallbackHandler(MemberInfo attemptedDestination, MethodInfo existingDestination, Exception detourException)
        {
            PsychologyBase.detoursMedical = false;
        }
    }
}
