using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Psychology
{
    public class Hediff_PlannedDate : Hediff
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref this.partner, "partner");
            Scribe_Values.LookValue(ref this.hour, "hour", 0);
            Scribe_Values.LookValue(ref this.day, "day", 0);
        }

        public override void Tick()
        {
            base.Tick();
            if(!LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.partner))
            {
                this.pawn.health.RemoveHediff(this);
            }
            else if(Find.TickManager.TicksGame >= this.day && GenLocalDate.HourOfDay(this.pawn) == this.hour)
            {
                if(this.pawn.GetTimeAssignment() != TimeAssignmentDefOf.Work && this.partner.GetTimeAssignment() != TimeAssignmentDefOf.Work && !this.pawn.Drafted && !this.partner.Drafted
                    && PartyUtility.AcceptableMapConditionsToStartParty(this.pawn.Map) && this.pawn.Map == this.partner.Map)
                {
                    pawn.jobs.StopAll();
                    if (pawn.Awake() && partner.Awake())
                    {
                        LordMaker.MakeNewLord(this.pawn.Faction, new LordJob_Date(this.pawn, this.partner), this.pawn.Map, new Pawn[] { this.pawn, this.partner });
                    }
                    else if(pawn.Awake())
                    {
                        this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.MissedDate, this.partner);
                    }
                    else
                    {
                        this.partner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.MissedDate, this.pawn);
                    }
                }
                else
                {
                    this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DateCancelled);
                    this.partner.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DateCancelled);
                }
                this.pawn.health.RemoveHediff(this);
            }
        }

        public Pawn partner;
        public int hour;
        public int day;
    }
}
