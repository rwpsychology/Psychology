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
    public class IncidentWorker_Election : IncidentWorker_MakeMapCondition
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            FactionBase factionBase = (FactionBase)Find.WorldObjects.ObjectsAt(map.Tile).ToList().Find(obj => obj is FactionBase);
            int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * GenDate.TicksPerDay);
            MapCondition cond = MapConditionMaker.MakeCondition(this.def.mapCondition, duration, 0);
            map.mapConditionManager.RegisterCondition(cond);
            Find.LetterStack.ReceiveLetter("LetterLabelNewElection".Translate(factionBase.Label), "LetterNewElection".Translate(factionBase.Label), LetterType.Good, new TargetInfo(map.Center, map, false), null);
            return true;
        }
    }
}
