using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak))]
    public static class MentalBreaker_AnxietyPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void AddAnxiety(MentalBreaker __instance, ref bool __result)
        {
            if (__result)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                int intensity;
                int.TryParse("" + (byte)Traverse.Create(__instance).Property("CurrentDesiredMoodBreakIntensity").GetValue<MentalBreakIntensity>(), out intensity);
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
                float PTSDChance = (0.25f - (0.075f * intensity));
                if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
                {
                    PTSDChance *= 0.75f;
                }
                if (PsycheHelper.PsychologyEnabled(pawn))
                {
                    //Laid-back pawns are less likely to get anxiety from mental breaks.
                    PTSDChance -= pawn.GetComp<CompPsychology>().Psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack) / 10f;
                }
                if (hediff != null)
                {
                    hediff.Severity += 0.15f - (intensity * 0.5f);
                }
                else if (Rand.Chance(PTSDChance))
                {
                    Hediff newHediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Anxiety, pawn, pawn.health.hediffSet.GetBrain());
                    newHediff.Severity = 0.75f - (intensity * 0.25f);
                    Letter newAnxiety = LetterMaker.MakeLetter("LetterLabelPTSD".Translate(), "LetterPTSD".Translate().AdjustedFor(pawn), LetterDefOf.NegativeEvent, pawn);
                    Find.LetterStack.ReceiveLetter(newAnxiety);
                    pawn.health.AddHediff(newHediff, null, null);
                }
            }
        }
    }
}
