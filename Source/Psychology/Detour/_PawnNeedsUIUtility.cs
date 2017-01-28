using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _PawnNeedsUIUtility
    {
        /* Stopgap fix for vanilla bug that Psychology exacerbates */
        [DetourMethod(typeof(PawnNeedsUIUtility), "GetThoughtGroupsInDisplayOrder")]
        internal static void _GetThoughtGroupsInDisplayOrder(Need_Mood mood, List<Thought> outThoughtGroupsPresent)
        {
            outThoughtGroupsPresent.Clear();
            outThoughtGroupsPresent.AddRange(mood.thoughts.DistinctThoughtGroups().Where(t => t.VisibleInNeedsTab));
            outThoughtGroupsPresent.SortByDescending((Thought th) => mood.thoughts.MoodOffsetOfThoughtGroup(th));
        }
    }
}
