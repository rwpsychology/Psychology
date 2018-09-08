using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(MentalState_SocialFighting), nameof(MentalState_SocialFighting.PostEnd))]
    public static class MentalState_SocialFighting_ThoughtPatch
    {
        [HarmonyPostfix]
        public static void WhoWon(MentalState_SocialFighting __instance, Pawn ___pawn, Pawn ___otherPawn)
        {
            float damage = ___pawn.health.summaryHealth.SummaryHealthPercent - ___otherPawn.health.summaryHealth.SummaryHealthPercent;
            if (damage > 0.05f)
            {
                ___pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.WonFight, ___otherPawn);
            }
            else if (damage < -0.05f)
            {
                ___otherPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.WonFight, ___pawn);
            }
        }
    }

    [HarmonyPatch(typeof(MentalState_SocialFighting), nameof(MentalState_SocialFighting.PostEnd))]
    public static class MentalState_SocialFighting_PersonalityPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPersonalityHook(IEnumerable<CodeInstruction> instrs)
        {
            float num;
            foreach(CodeInstruction itr in instrs)
            {
                if(itr.opcode == OpCodes.Ldc_R4 && float.TryParse("" + itr.operand, out num) && num == 0.5f)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MentalState_SocialFighting), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MentalState_SocialFighting_PersonalityPatch), nameof(MentalState_SocialFighting_PersonalityPatch.PersonalityChance), new Type[] { typeof(Pawn) }));
                }
                else
                {
                    yield return itr;
                }
            }
        }

        public static float PersonalityChance(Pawn pawn)
        {
            if(PsycheHelper.PsychologyEnabled(pawn))
            {
                return 1f - PsycheHelper.Comp(pawn).Psyche.GetPersonalityNodeOfDef(PersonalityNodeDefOf.Aggressive).AdjustedRating;
            }
            return 0.5f;
        }
    }
}
