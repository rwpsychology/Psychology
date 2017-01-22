using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class IncidentWorker_CliquesForm : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Pawn pawn = map.mapPawns.FreeColonistsSpawned.RandomElement();
            if (pawn == null)
            {
                return false;
            }
            List<Pawn> enemies = (from p in map.mapPawns.FreeColonistsSpawned
                                  where p != pawn && p.relations.OpinionOf(pawn) < -20 && pawn.relations.OpinionOf(p) < -20
                                  select p).ToList();
            if (enemies.Count == 0)
            {
                return false;
            }
            Pawn enemy = enemies.RandomElement<Pawn>();
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Clique, enemy);
            enemy.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.Clique, pawn);
            Find.LetterStack.ReceiveLetter("LetterLabelCliques".Translate(), "CliquesFormed".Translate(new object[] { pawn.LabelShort, enemy.LabelShort }), LetterType.BadNonUrgent, pawn, null);
            return true;
        }
    }
}
