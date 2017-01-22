using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Alert_CliqueMembers : Alert
    {
        public Alert_CliqueMembers()
        {
            this.defaultLabel = "CliqueMembers".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        public override string GetExplanation()
        {
            List<Pawn>[] cliques = new List<Pawn>[CliqueLeaders.Count()];
            int leaderNum = 0;
            foreach (Pawn leader in CliqueLeaders)
            {
                List<Pawn> members = (from c in PawnsFinder.AllMaps_FreeColonistsSpawned
                                      where c != leader && c.Map == leader.Map && c.relations.OpinionOf(leader) > 20
                                      select c).ToList();
                members.Insert(0, leader);
                cliques[leaderNum] = members;
                leaderNum++;
            }
            StringBuilder cliqueList = new StringBuilder();
            foreach(List<Pawn> clique in cliques)
            {
                leaderNum = 0;
                foreach (Pawn member in clique)
                {
                    cliqueList.AppendLine(leaderNum == 0 ? "Leader: " + member.LabelShort : member.LabelShort);
                    leaderNum++;
                }
                cliqueList.AppendLine();
            }
            return "CliqueMembersDesc".Translate(cliqueList);
        }

        public override AlertReport GetReport()
        {
            if(CliqueLeaders.Count() < 2)
            {
                return AlertReport.Inactive;
            }
            return AlertReport.Active;
        }

        private IEnumerable<Pawn> CliqueLeaders
        {
            get
            {
                return (from c in PawnsFinder.AllMaps_FreeColonistsSpawned
                        where c.needs.mood.thoughts.memories.NumMemoryThoughtsOfDef(ThoughtDefOfPsychology.Clique) > 0
                        select c);
            }
        }
    }
}
