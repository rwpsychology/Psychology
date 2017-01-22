using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class IncidentWorker_Homesick : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned
                               where p.RaceProps.Humanlike && p.Faction.IsPlayer && p.Awake()
                               select p).ToList();
            if (list.Count == 0)
            {
                return false;
            }
            Pawn pawn = list.RandomElement();
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Homesickness, null);
            Find.LetterStack.ReceiveLetter("LetterLabelHomesick".Translate(), "ColonistHomesick".Translate(new object[] { pawn.LabelShort }).AdjustedFor(pawn), LetterType.BadNonUrgent, pawn, null);
            return true;
        }
    }
}
