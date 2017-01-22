using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _ThingSelectionUtility
    {
        [DetourMethod(typeof(ThingSelectionUtility),"SelectableByMapClick")]
        internal static bool SelectableByMapClick(Thing t)
        {
            if (!t.def.selectable)
            {
                return false;
            }
            if (!t.Spawned)
            {
                return false;
            }
            if (t is Pawn && ((Pawn)t).health.hediffSet.HasHediff(HediffDefOfPsychology.Thief))
            {
                return false;
            }
            if (t.def.size.x == 1 && t.def.size.z == 1)
            {
                return !t.Position.Fogged(t.Map);
            }
            CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
            while (!iterator.Done())
            {
                if (!iterator.Current.Fogged(t.Map))
                {
                    return true;
                }
                iterator.MoveNext();
            }
            return false;
        }
    }
}
