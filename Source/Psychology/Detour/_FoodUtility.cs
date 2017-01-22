using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _FoodUtility
    {
        // Token: 0x06000037 RID: 55 RVA: 0x00003978 File Offset: 0x00001B78
        [DetourMethod(typeof(FoodUtility),"ThoughtsFromIngesting")]
        internal static List<ThoughtDef> _ThoughtsFromIngesting(Pawn ingester, Thing t)
        {
            ingestThoughts.Clear();
            if (ingester.needs == null || ingester.needs.mood == null)
            {
                return ingestThoughts;
            }
            ThingDef thingDef = t.def;
            if (thingDef == ThingDefOf.NutrientPasteDispenser)
            {
                thingDef = ThingDefOf.MealNutrientPaste;
            }
            if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && thingDef.ingestible.tasteThought != null)
            {
                ingestThoughts.Add(thingDef.ingestible.tasteThought);
            }
            CompIngredients compIngredients = t.TryGetComp<CompIngredients>();
            if (FoodUtility.IsHumanlikeMeat(thingDef) && ingester.RaceProps.Humanlike)
            {
                ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatDirect : ThoughtDefOf.AteHumanlikeMeatDirectCannibal);
            }
            else if (compIngredients != null)
            {
                for (int i = 0; i < compIngredients.ingredients.Count; i++)
                {
                    ThingDef thingDef2 = compIngredients.ingredients[i];
                    if (thingDef2.ingestible != null)
                    {
                        if (ingester.RaceProps.Humanlike && FoodUtility.IsHumanlikeMeat(thingDef2))
                        {
                            ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatAsIngredient : ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal);
                        }
                        else if (thingDef2.ingestible.specialThoughtAsIngredient != null)
                        {
                            ingestThoughts.Add(thingDef2.ingestible.specialThoughtAsIngredient);
                        }
                    }
                }
            }
            else if (thingDef.ingestible.specialThoughtDirect != null)
            {
                ingestThoughts.Add(thingDef.ingestible.specialThoughtDirect);
            }
            if (t.IsNotFresh())
            {
                ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
            }
            List<ThoughtDef> newThoughts = new List<ThoughtDef>();
            foreach (var thought in ingestThoughts)
                if (DefDatabase<ThoughtDef>.GetNamedSilentFail(thought.defName + "PickyEater") != null)
                    newThoughts.Add(ThoughtDef.Named(thought.defName + "PickyEater"));
            ingestThoughts.AddRange(newThoughts);
            return ingestThoughts;
        }

        private static List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();
    }
}
