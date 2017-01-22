using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _RecordsUtility
    {
        [DetourMethod(typeof(RecordsUtility),"Notify_BillDone")]
        internal static void _Notify_BillDone(Pawn billDoer, List<Thing> products)
        {
            for (int i = 0; i < products.Count; i++)
            {
                var ShouldIncrementThingsCrafted = typeof(RecordsUtility).GetMethod("ShouldIncrementThingsCrafted", BindingFlags.Static | BindingFlags.NonPublic);
                if (products[i].def.IsNutritionGivingIngestible && products[i].def.ingestible.preferability >= FoodPreferability.MealAwful)
                {
                    billDoer.records.Increment(RecordDefOf.MealsCooked);
                    billDoer.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.CookedMealBleedingHeart, (Pawn)null);
                }
                else if (ShouldIncrementThingsCrafted != null && (bool)ShouldIncrementThingsCrafted.Invoke(null, new object[] { products[i] }))
                {
                    billDoer.records.Increment(RecordDefOf.ThingsCrafted);
                }
                else if (ShouldIncrementThingsCrafted == null)
                    Log.ErrorOnce("Unable to reflect RecordsUtility.ShouldIncrementThingsCrafted!", 305432421);
            }
        }
    }
}
