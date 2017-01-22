// Decompiled with JetBrains decompiler
// Type: RimWorld.TendUtility
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _TendUtility
    {
        private static List<Hediff_MissingPart> bleedingStumps = new List<Hediff_MissingPart>();
        private static List<Hediff> otherHediffs = new List<Hediff>();

        [DetourMethod(typeof(TendUtility),"DoTend")]
        internal static void _DoTend(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (!patient.health.HasHediffsNeedingTend(false))
            {
                return;
            }
            if (medicine != null && medicine.Destroyed)
            {
                Log.Warning("Tried to use destroyed medicine.");
                medicine = null;
            }
            float num = (medicine == null) ? 0.2f : medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
            float num2 = num;
            Building_Bed building_Bed = patient.CurrentBed();
            if (building_Bed != null)
            {
                num2 += building_Bed.GetStatValue(StatDefOf.MedicalTendQualityOffset, true);
            }
            if (doctor != null)
            {
                num2 *= doctor.GetStatValue(StatDefOf.HealingQuality, true);
            }
            num2 = Mathf.Clamp01(num2);
            if (patient.health.hediffSet.GetInjuriesTendable().Any<Hediff_Injury>())
            {
                float num3 = 0f;
                int num4 = 0;
                foreach (Hediff_Injury current in from x in patient.health.hediffSet.GetInjuriesTendable()
                                                  orderby x.Severity descending
                                                  select x)
                {
                    float num5 = Mathf.Min(current.Severity, 20f);
                    if (num3 + num5 > 20f)
                    {
                        break;
                    }
                    num3 += num5;
                    current.Tended(num2, num4);
                    if (medicine == null)
                    {
                        break;
                    }
                    num4++;
                }
            }
            else
            {
                bleedingStumps.Clear();
                List<Hediff_MissingPart> missingPartsCommonAncestors = patient.health.hediffSet.GetMissingPartsCommonAncestors();
                for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
                {
                    if (missingPartsCommonAncestors[i].IsFresh)
                    {
                        bleedingStumps.Add(missingPartsCommonAncestors[i]);
                    }
                }
                if (bleedingStumps.Count > 0)
                {
                    bleedingStumps.RandomElement<Hediff_MissingPart>().IsFresh = false;
                    bleedingStumps.Clear();
                }
                else
                {
                    otherHediffs.Clear();
                    otherHediffs.AddRange(patient.health.hediffSet.GetTendableNonInjuryNonMissingPartHediffs());
                    Hediff hediff;
                    if (otherHediffs.TryRandomElement(out hediff))
                    {
                        HediffCompProperties_TendDuration hediffCompProperties_TendDuration = hediff.def.CompProps<HediffCompProperties_TendDuration>();
                        if (hediffCompProperties_TendDuration != null && hediffCompProperties_TendDuration.tendAllAtOnce)
                        {
                            int num6 = 0;
                            for (int j = 0; j < otherHediffs.Count; j++)
                            {
                                if (otherHediffs[j].def == hediff.def)
                                {
                                    otherHediffs[j].Tended(num2, num6);
                                    num6++;
                                }
                            }
                        }
                        else
                        {
                            hediff.Tended(num2, 0);
                        }
                    }
                    otherHediffs.Clear();
                }
            }
            if (doctor != null && patient.HostFaction == null && patient.Faction != null && patient.Faction != doctor.Faction)
            {
                patient.Faction.AffectGoodwillWith(doctor.Faction, 0.3f);
            }
            if (doctor != null && doctor.RaceProps.Humanlike && patient.RaceProps.Animal && RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.004f) && doctor.Faction != null && doctor.Faction != patient.Faction)
            {
                InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, 1f, false);
            }
            patient.records.Increment(RecordDefOf.TimesTendedTo);
            if (doctor != null)
            {
                doctor.records.Increment(RecordDefOf.TimesTendedOther);
                doctor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DoctorBleedingHeart, patient);
            }
            if (medicine != null)
            {
                if ((patient.Spawned || (doctor != null && doctor.Spawned)) && num > 1f)
                {
                    SoundDef.Named("TechMedicineUsed").PlayOneShot(new TargetInfo(patient.Position, patient.Map, false));
                }
                if (medicine.stackCount > 1)
                {
                    medicine.stackCount--;
                }
                else if (!medicine.Destroyed)
                {
                    medicine.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
