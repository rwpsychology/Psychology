using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class IncidentWorker_Quarrel : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned
                               where p.RaceProps.Humanlike && p.Faction.IsPlayer
                               select p).ToList();
            if (list.Count == 0)
            {
                return false;
            }
            Pawn pawn = list.RandomElement();
            List<Pawn> friendlies = (from p in map.mapPawns.AllPawnsSpawned
                               where p.RaceProps.Humanlike && p.Faction.IsPlayer && p != pawn && p.relations.OpinionOf(pawn) > 20 && pawn.relations.OpinionOf(p) > 20
                                     select p).ToList();
            if (friendlies.Count == 0)
            {
                return false;
            }
            Pawn friend = friendlies.RandomElement();
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Quarrel, friend);
            friend.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Quarrel, pawn);
            Find.LetterStack.ReceiveLetter("LetterLabelQuarrel".Translate(), "ColonistsQuarrel".Translate(new object[] { pawn.LabelShort, friend.LabelShort }), LetterType.BadNonUrgent, pawn, null);
            return true;
        }
    }
}
