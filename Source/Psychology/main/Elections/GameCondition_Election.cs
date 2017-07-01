using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class GameCondition_Election : GameCondition
    {
        public override void Init()
        {
            base.Init();
            //Make sure the election occurs during the day if possible.
            int plannedStart = GenDate.HourOfDay(this.duration + Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(this.Map.Tile).x);
            if(plannedStart < 7)
            {
                this.duration += (7 - plannedStart) * GenDate.TicksPerHour;
            }
            else if (plannedStart > 18)
            {
                this.duration -= (plannedStart - 18) * GenDate.TicksPerHour;
            }
            List<PsychologyPawn> psychologyColonists = (from c in this.Map.mapPawns.FreeColonistsSpawned
                                                        where c is PsychologyPawn
                                                        select (PsychologyPawn)c).ToList();
            int maxCandidatesThisColonySupports = Mathf.RoundToInt(psychologyColonists.Count() * 0.3f);
            float totalOutspoken = 0f;
            psychologyColonists.ForEach(p => totalOutspoken += p.psyche.GetPersonalityRating(PersonalityNodeDefOf.Outspoken));
            int numCandidates = Rand.RangeInclusive(Mathf.Min(maxCandidatesThisColonySupports, 1 + Mathf.RoundToInt(totalOutspoken * 0.1f)), maxCandidatesThisColonySupports);
            int tries = 0;
            while (this.candidates.Count < numCandidates && tries < 500)
            {
                PsychologyPawn candidate = psychologyColonists.RandomElementByWeight(p => p.psyche.GetPersonalityRating(PersonalityNodeDefOf.Outspoken) * 2 + (p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) ? p.needs.mood.CurLevel - 0.5f : 0f));
                List<PersonalityNodeDef> issues = new List<PersonalityNodeDef>();
                int tries2 = 0;
                while(issues.Count < 5 && tries2 < 500)
                {
                    PersonalityNodeDef issue = (from node in candidate.psyche.PersonalityNodes
                                                where !node.Core
                                                select node.def).RandomElementByWeight(n => Mathf.Pow(Mathf.Abs(0.5f - candidate.psyche.GetPersonalityRating(n)),2) * Mathf.Pow(2, n.controversiality));
                    if(!issues.Contains(issue))
                    {
                        issues.Add(issue);
                    }
                    else
                    {
                        tries2++;
                    }
                }
                if(issues.Count >= 5 && this.candidates.Find(c => c.pawn == candidate) == null)
                {
                    this.candidates.Add(new Candidate(candidate, issues));
                }
                else
                {
                    if(issues.Count < 5)
                    {
                        Log.Error("[Psychology] Could not find five unique issues for " + candidate.LabelShort + "'s platform.");
                    }
                    tries++;
                }
            }
            if(candidates.Count == 0)
            {
                this.End();
                Log.Error("[Psychology] Tried to start election but could not find anyone to run.");
                return;
            }
            foreach (Candidate candidate in candidates)
            {
                StringBuilder issuesString = new StringBuilder();
                for (int i = 0; i < candidate.nodes.Count; i++)
                {
                    issuesString.AppendFormat("{0}) {1}{2}",i+1,candidate.pawn.psyche.GetPersonalityNodeOfDef(candidate.nodes[i]).PlatformIssue,(i != candidate.nodes.Count-1 ? "\n" : ""));
                }
                Find.LetterStack.ReceiveLetter("LetterLabelElectionCandidate".Translate(candidate.pawn.LabelShort), "LetterElectionCandidate".Translate(candidate.pawn.LabelShort, ((FactionBase)Find.WorldObjects.ObjectsAt(candidate.pawn.Map.Tile).ToList().Find(obj => obj is FactionBase)).Label, issuesString.ToString()).AdjustedFor(candidate.pawn), LetterDefOf.Good, candidate.pawn, null);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.candidates, "candidates", LookMode.Deep);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            foreach(Candidate candidate in candidates)
            {
                if(candidate.pawn.Dead)
                {
                    candidates.Remove(candidate);
                }
            }
        }

        public override void End()
        {
            base.End();
            if(candidates.Count == 0)
            {
                return;
            }
            IntVec3 intVec;
            PsychologyPawn organizer = candidates.RandomElement().pawn;
            string baseName = ((FactionBase)Find.WorldObjects.ObjectsAt(organizer.Map.Tile).ToList().Find(obj => obj is FactionBase)).Label;
            if (!RCellFinder.TryFindPartySpot(organizer, out intVec))
            {
                return;
            }
            LordMaker.MakeNewLord(organizer.Faction, new LordJob_Joinable_Election(intVec, candidates, baseName), organizer.Map, null);
            Find.LetterStack.ReceiveLetter("LetterLabelElectionHeld".Translate(baseName), "LetterElectionHeld".Translate(baseName), LetterDefOf.Good, new TargetInfo(intVec, organizer.Map, false), null);
        }
        
        public List<Candidate> candidates = new List<Candidate>();
    }
}
