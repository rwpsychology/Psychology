// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtUtility
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _PawnDiedOrDownedThoughtsUtility
    {
        // Token: 0x06000D35 RID: 3381 RVA: 0x00041134 File Offset: 0x0003F334
        [DetourMethod(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Humanlike")]
        private static void _AppendThoughts_Humanlike(Pawn victim, DamageInfo? dinfo, Hediff hediff, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
        {
            var IsExecution = typeof(PawnDiedOrDownedThoughtsUtility).GetMethod("IsExecution", BindingFlags.Static | BindingFlags.NonPublic);
            var IsInnocentPrisoner = typeof(PawnDiedOrDownedThoughtsUtility).GetMethod("IsInnocentPrisoner", BindingFlags.Static | BindingFlags.NonPublic);
            bool flag = (bool)IsExecution.Invoke(null, new object[] { dinfo, hediff });
            bool flag2 = (bool)IsInnocentPrisoner.Invoke(null, new object[] { victim });
            bool flag3 = dinfo.HasValue && dinfo.Value.Def.externalViolence && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn;
            if (flag3)
            {
                Pawn pawn = (Pawn)dinfo.Value.Instigator;
                if (!pawn.Dead && pawn.needs.mood != null && pawn.story != null && pawn != victim)
                {
                    if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
                    {
                        outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, pawn, null, 1f, 1f));
                    }
                    if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(pawn))
                    {
                        if (victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(pawn.Faction))
                        {
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, pawn, victim, 1f, 1f));
                        }
                        else if(victim.Faction != null && victim.Faction.HostileTo(pawn.Faction) && !flag2)
                        {
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOfPsychology.KilledHumanlikeEnemy, pawn, victim, 1f, 1f));
                        }
                        if (victim.kindDef.combatPower > 250f)
                        {
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedMajorEnemy, pawn, victim, 1f, 1f));
                        }
                    }
                }
            }
            if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.Spawned)
            {
                List<Pawn> allPawnsSpawned = victim.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    Pawn pawn2 = allPawnsSpawned[i];
                    if (pawn2 != victim && pawn2.needs.mood != null)
                    {
                        if (!flag && (pawn2.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)pawn2.MentalState).otherPawn != victim))
                        {
                            if (pawn2.Position.InHorDistOf(victim.Position, 12f) && GenSight.LineOfSight(victim.Position, pawn2.Position, victim.Map, false) && pawn2.Awake() && pawn2.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                            {
                                if (pawn2.Faction == victim.Faction)
                                {
                                    outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathAlly, pawn2, null, 1f, 1f));
                                    outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOfPsychology.WitnessedDeathAllyBleedingHeart, pawn2, null, 1f, 1f));
                                }
                                else if (victim.Faction == null || (!victim.Faction.HostileTo(pawn2.Faction) || pawn2.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart)))
                                {
                                    outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathNonAlly, pawn2, null, 1f, 1f));
                                    outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOfPsychology.WitnessedDeathNonAllyBleedingHeart, pawn2, null, 1f, 1f));
                                }
                                if (pawn2.relations.FamilyByBlood.Contains(victim))
                                {
                                    outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathFamily, pawn2, null, 1f, 1f));
                                }
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathBloodlust, pawn2, null, 1f, 1f));
                                if (!pawn2.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart) && !pawn2.story.traits.HasTrait(TraitDefOf.Psychopath) && !pawn2.story.traits.HasTrait(TraitDefOf.Bloodlust) && !pawn2.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
                                {
                                    if (((pawn2.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn2) + GenLocalDate.Year(pawn2) + (int)(GenLocalDate.DayPercent(pawn2) * 5) * 60) * 391) % 1000) == 0)
                                    {
                                        pawn2.story.traits.GainTrait(new Trait(TraitDefOfPsychology.Desensitized));
                                        pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.RecentlyDesensitized, null);
                                    }
                                }
                            }
                            else if (victim.Faction == Faction.OfPlayer && victim.Faction == pawn2.Faction && victim.HostFaction != pawn2.Faction)
                            {
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowColonistDied, pawn2, null, 1f, 1f));
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOfPsychology.KnowColonistDiedBleedingHeart, pawn2, null, 1f, 1f));
                            }
                            if (flag2 && pawn2.Faction == Faction.OfPlayer && !pawn2.IsPrisoner)
                            {
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowPrisonerDiedInnocent, pawn2, null, 1f, 1f));
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOfPsychology.KnowPrisonerDiedInnocentBleedingHeart, pawn2, null, 1f, 1f));
                            }
                        }
                    }
                }
            }
            if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Abandoned && victim.IsColonist)
            {
                outAllColonistsThoughts.Add(ThoughtDefOf.ColonistAbandoned);
                outAllColonistsThoughts.Add(ThoughtDefOfPsychology.ColonistAbandonedBleedingHeart);
            }
            if (thoughtsKind == PawnDiedOrDownedThoughtsKind.AbandonedToDie)
            {
                if (victim.IsColonist)
                {
                    outAllColonistsThoughts.Add(ThoughtDefOf.ColonistAbandonedToDie);
                    outAllColonistsThoughts.Add(ThoughtDefOfPsychology.ColonistAbandonedToDieBleedingHeart);
                }
                else if (victim.IsPrisonerOfColony)
                {
                    outAllColonistsThoughts.Add(ThoughtDefOf.PrisonerAbandonedToDie);
                    outAllColonistsThoughts.Add(ThoughtDefOfPsychology.PrisonerAbandonedToDieBleedingHeart);
                }
            }
        }
    }
}
