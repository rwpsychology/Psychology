using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Psychology
{
    public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
    {
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
        }
        
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }
        
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = null;
            switch (parms.raidArrivalMode)
            {
                case PawnsArriveMode.EdgeWalkIn:
                    text = "EnemyRaidWalkIn".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
                case PawnsArriveMode.EdgeDrop:
                    text = "EnemyRaidEdgeDrop".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
                case PawnsArriveMode.CenterDrop:
                    text = "EnemyRaidCenterDrop".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
            }
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "EnemyRaidLeaderPresent".Translate(new object[]
                {
                    pawn.Faction.def.pawnsPlural,
                    pawn.LabelShort
                });
            }
            return text;
        }

        protected override LetterType GetLetterType()
        {
            return LetterType.BadUrgent;
        }
        
        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(new object[]
            {
                parms.faction.def.pawnsPlural
            });
        }
        
        protected override void ResolveRaidStrategy(IncidentParms parms)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            Map map = (Map)parms.target;
            parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
                                  where d.Worker.CanUseWith(parms)
                                  select d).RandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionChance(map));
        }

        public override bool TryExecute(IncidentParms parms)
        {
            if (!base.TryExecute(parms))
            {
                return false;
            }
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            List<Pawn> saboteurs = (from p in ((Map)parms.target).mapPawns.AllPawnsSpawned
                                    where p.RaceProps.Humanlike && p.Faction.IsPlayer && p.health.hediffSet.HasHediff(HediffDefOfPsychology.Saboteur)
                                    select p).ToList();
            if(saboteurs.Count > 0)
            {
                Pawn saboteur = saboteurs.RandomElement();
                if(Rand.Value < 0.33f)
                {
                    saboteur.health.hediffSet.hediffs.RemoveAll(h => h.def == HediffDefOfPsychology.Saboteur);
                    Faction oldFaction = saboteur.Faction;
                    saboteur.SetFaction(parms.faction);
                    Lord enemyLord = saboteur.Map.lordManager.lords.Find((Lord x) => x.faction == parms.faction);
                    enemyLord.ownedPawns.Add(saboteur);
                    saboteur.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                    Find.LetterStack.ReceiveLetter("LetterLabelSabotage".Translate(), "SaboteurRevealedFaction".Translate(new object[] { saboteur.LabelShort, parms.faction.Name }).AdjustedFor(saboteur), LetterType.BadUrgent, saboteur, null);
                }
            }
            return true;
        }
        
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (parms.faction != null)
            {
                return true;
            }
            float maxPoints = parms.points;
            if (maxPoints <= 0f)
            {
                maxPoints = 999999f;
            }
            if (!(from f in Find.FactionManager.AllFactions
                  where this.FactionCanBeGroupSource(f, map, false) && maxPoints >= f.def.MinPointsToGenerateNormalPawnGroup()
                  select f).TryRandomElementByWeight((Faction f) => f.def.raidCommonality, out parms.faction))
            {
                if (!(from f in Find.FactionManager.AllFactions
                      where this.FactionCanBeGroupSource(f, map, true) && maxPoints >= f.def.MinPointsToGenerateNormalPawnGroup()
                      select f).TryRandomElementByWeight((Faction f) => f.def.raidCommonality, out parms.faction))
                {
                    Log.Error("IncidentWorker_RaidEnemy could not fire even though we thought we could: no faction could generate with " + maxPoints + " points.");
                    return false;
                }
            }
            return true;
        }
    }
}
