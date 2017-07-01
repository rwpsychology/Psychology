 using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnObserver), "ObserveSurroundingThings")]
    public static class PawnObserver_ObserveSurroundingPatch
    {
        [HarmonyPostfix]
        public static void DesensitizeViaCorpse(PawnObserver __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                return;
            }
            RoomGroup roomGroup = pawn.GetRoomGroup();
            Map map = pawn.Map;
            int num = 0;
            while ((float)num < 100f)
            {
                IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[num];
                if (intVec.InBounds(map))
                {
                    if (intVec.GetRoomGroup(map) == roomGroup)
                    {
                        if (GenSight.LineOfSight(intVec, pawn.Position, map, true, null, 0, 0))
                        {
                            List<Thing> thingList = intVec.GetThingList(map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                IThoughtGiver thoughtGiver = thingList[i] as IThoughtGiver;
                                if (thoughtGiver != null)
                                {
                                    Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought();
                                    if (thought_Memory != null && thought_Memory.def == ThoughtDefOf.ObservedLayingCorpse)
                                    {
                                        if (!pawn.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart) && !pawn.story.traits.HasTrait(TraitDefOf.Psychopath) && !pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
                                        {
                                            if (((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % 1000) == 0)
                                            {
                                                pawn.story.traits.GainTrait(new Trait(TraitDefOfPsychology.Desensitized));
                                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RecentlyDesensitized, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                num++;
            }
        }
    }
}
