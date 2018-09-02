 using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnObserver), "ObserveSurroundingThings")]
    public static class PawnObserver_ObserveSurroundingPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DesensitizeViaCorpse(IEnumerable<CodeInstruction> instrs)
        {
            foreach(CodeInstruction itr in instrs)
            {
                yield return itr;
                if(itr.opcode == OpCodes.Callvirt && itr.operand == AccessTools.Method(typeof(IThoughtGiver), nameof(IThoughtGiver.GiveObservedThought), new Type[] { }))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnObserver), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnObserver_ObserveSurroundingPatch), nameof(PawnObserver_ObserveSurroundingPatch.AddDesensitizedChance), new Type[] { typeof(Thought_Memory), typeof(Pawn) }));
                }
            }
        }

        public static Thought_Memory AddDesensitizedChance(Thought_Memory thought_Memory, Pawn pawn)
        {
            if (thought_Memory != null && thought_Memory.def == ThoughtDefOf.ObservedLayingCorpse)
            {
                if (!pawn.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart) && !pawn.story.traits.HasTrait(TraitDefOf.Psychopath) && !pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
                {
                    if (((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % 1000) == 0)
                    {
                        pawn.story.traits.GainTrait(new Trait(TraitDefOfPsychology.Desensitized));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RecentlyDesensitized);
                    }
                }
            }
            return thought_Memory;
        }
    }
}
