using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class IncidentWorker_Election : IncidentWorker_MakeGameCondition
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            FactionBase factionBase = Find.WorldObjects.ObjectsAt(map.Tile).OfType<FactionBase>().First();
            int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * GenDate.TicksPerDay);
            GameCondition cond = GameConditionMaker.MakeCondition(this.def.gameCondition, duration, 0);
            map.gameConditionManager.RegisterCondition(cond);
            Find.LetterStack.ReceiveLetter("LetterLabelNewElection".Translate(factionBase.Label), "LetterNewElection".Translate(factionBase.Label), LetterDefOf.Good, new TargetInfo(map.Center, map, false), null);
            return true;
        }
    }
}
