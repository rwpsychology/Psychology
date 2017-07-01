using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(RecordsUtility), "Notify_BillDone")]
    public static class RecordsUtility_BillDonePatch
    {
        [HarmonyPostfix]
        public static void BleedingHeartThought(Pawn billDoer, List<Thing> products)
        {
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].def.IsNutritionGivingIngestible && products[i].def.ingestible.preferability >= FoodPreferability.MealAwful)
                {
                    if(billDoer.needs != null)
                    {
                        billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CookedMealBleedingHeart, (Pawn)null);
                    }
                }
            }
        }
    }
}
