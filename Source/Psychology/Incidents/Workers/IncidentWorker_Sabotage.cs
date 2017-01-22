using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    class IncidentWorker_Sabotage : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned
                               where p.RaceProps.Humanlike && p.Faction.IsPlayer
                               select p).ToList();
            if (list.Count < 3 || !PsychologyBase.SabotageOn())
            {
                return false;
            }
            Pawn pawn = list.RandomElement();
            Hediff hediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Saboteur, pawn, null);
            pawn.health.AddHediff(hediff, null, null);
            return true;
        }
    }
}
