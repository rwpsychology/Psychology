using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class MentalStateWorker_FellPlotting : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            return base.StateCanOccur(pawn) && !pawn.story.WorkTagIsDisabled(WorkTags.Violent) && pawn.Map.mapPawns.FreeColonistsSpawned.ToList().Find((Pawn x) => pawn.relations.OpinionOf(x) < -20) != null;
        }
    }
}
