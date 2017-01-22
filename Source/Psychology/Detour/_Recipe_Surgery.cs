// Decompiled with JetBrains decompiler
// Type: RimWorld.Recipe_MedicalOperation
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

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
            float num = 1f;
            float num2 = surgeon.GetStatValue(StatDefOf.SurgerySuccessChance, true);
            if (num2 < 1f)
            {
                num2 = Mathf.Pow(num2, s.recipe.surgeonSurgerySuccessChanceExponent);
            }
            num *= num2;
            Room room = surgeon.GetRoom();
            if (room != null)
            {
                float num3 = room.GetStat(RoomStatDefOf.SurgerySuccessChanceFactor);
                if (num3 < 1f)
                {
                    num3 = Mathf.Pow(num3, s.recipe.roomSurgerySuccessChanceFactorExponent);
                }
                num *= num3;
            }
            var GetAverageMedicalPotency = typeof(Recipe_Surgery).GetMethod("GetAverageMedicalPotency", BindingFlags.Instance | BindingFlags.NonPublic);
            if (GetAverageMedicalPotency != null)
                num *= (float)GetAverageMedicalPotency.Invoke(s, new object[] { ingredients });
            num *= s.recipe.surgerySuccessChanceFactor;
            if (Rand.Value > num)
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
