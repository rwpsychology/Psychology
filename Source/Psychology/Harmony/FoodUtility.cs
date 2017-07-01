using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class FoodUtility_AddPickyThoughts_Patch
    {
        [HarmonyPostfix]
        public static void AddPickyThoughtsPatch(ref List<ThoughtDef> __result)
        {
            List<ThoughtDef> newThoughts = new List<ThoughtDef>();
            foreach (var thought in __result)
                if (DefDatabase<ThoughtDef>.GetNamedSilentFail(thought.defName + "PickyEater") != null)
                    newThoughts.Add(ThoughtDef.Named(thought.defName + "PickyEater"));
            __result.AddRange(newThoughts);
        }
    }
}
