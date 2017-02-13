using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_Individuality : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (p.Map == null)
                return ThoughtState.Inactive;
            if (!p.IsColonist)
                return ThoughtState.Inactive;
            if (p.apparel.PsychologicallyNude)
                return ThoughtState.Inactive;
            if (GenDate.MonthsPassed <= 6)
                return ThoughtState.Inactive;
            if (!PsychologyBase.IndividualityOn())
                return ThoughtState.Inactive;
            if (!lastTick.ContainsKey(p) || Find.TickManager.TicksGame - 250 > lastTick[p][0])
            {
                Func<Apparel, bool> identical = delegate (Apparel x)
                {
                    foreach (Apparel a in p.apparel.WornApparel)
                    {
                        if (a.def == x.def && a.Stuff == x.Stuff && a.DrawColor == x.DrawColor)
                            return true;
                    }
                    return false;
                };
                List<Pawn> colonists = (from c in p.Map.mapPawns.FreeColonistsSpawned
                                        where c != p
                                        select c).ToList();
                List<Pawn> sameClothes = (from c in colonists
                                          where (from a in c.apparel.WornApparel
                                                 where identical(a)
                                                 select a).ToList().Count == p.apparel.WornApparelCount
                                          select c).ToList();
                if (sameClothes.Count == colonists.Count && colonists.Count > 5)
                {
                    lastTick[p] = new int[] { Find.TickManager.TicksGame, 3 };
                }
                else if (sameClothes.Count >= (colonists.Count / 2) && colonists.Count > 5)
                {
                    lastTick[p] = new int[] { Find.TickManager.TicksGame, 2 };
                }
                else if (sameClothes.Count > 1)
                {
                    lastTick[p] = new int[] { Find.TickManager.TicksGame, 1 };
                }
                else if (sameClothes.Count > 0)
                {
                    lastTick[p] = new int[] { Find.TickManager.TicksGame, 0 };
                }
                else
                {
                    lastTick[p] = new int[] { Find.TickManager.TicksGame, -1 };
                }
            }
            if (lastTick[p][1] >= 0)
            {
                return ThoughtState.ActiveAtStage(lastTick[p][1]);
            }
            return ThoughtState.Inactive;
        }

        Dictionary<Pawn, int[]> lastTick = new Dictionary<Pawn, int[]>();
    }
}
