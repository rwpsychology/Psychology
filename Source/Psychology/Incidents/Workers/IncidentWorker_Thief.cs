using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    class IncidentWorker_Thief : IncidentWorker
    {
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Faction player = Find.FactionManager.AllFactions.ToList().Find((Faction x) => x.IsPlayer);
            Pawn thief = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("Scavenger"), Find.FactionManager.RandomEnemyFaction());
            IntVec3 intVec;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map))
            {
                return false;
            }
            GenSpawn.Spawn(thief, intVec, map);
            Hediff hediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Thief, thief, null);
            thief.health.AddHediff(hediff);
            IntVec3 c;
            if (!RCellFinder.TryFindBestExitSpot(thief, out c, TraverseMode.ByPawn))
            {
                return false;
            }
            List<Thing> valuables = (from t in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                                     where (t.MarketValue*Mathf.Min(t.stackCount, (int)(thief.GetStatValue(StatDefOf.CarryingCapacity, true) / t.def.VolumePerUnit))) >= 250 && HaulAIUtility.PawnCanAutomaticallyHaulFast(thief, t)
                                     select t).ToList();
            if (valuables.Count == 0)
            {
                thief.Destroy();
                return false;
            }
            Thing valuable = valuables.RandomElement();
            thief.jobs.StartJob(new Job(JobDefOf.Steal) {
                    targetA = valuable,
                    targetB = c,
                    count = Mathf.Min(valuable.stackCount, (int)(thief.GetStatValue(StatDefOf.CarryingCapacity, true) / valuable.def.VolumePerUnit))
                });
            return true;
        }
    }
}
