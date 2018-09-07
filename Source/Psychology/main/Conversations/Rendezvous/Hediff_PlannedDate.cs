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
            Scribe_References.Look(ref this.partner, "partner");
            Scribe_Values.Look(ref this.hour, "hour", 0);
            Scribe_Values.Look(ref this.day, "day", 0);
        }

        [LogPerformance]
        public override void Tick()
        {
            base.Tick();
            if(!LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.partner))
            {
                this.pawn.health.RemoveHediff(this);
            }
            else if(Find.TickManager.TicksAbs >= this.day && GenLocalDate.HourOfDay(this.pawn) == this.hour)
            {
                if(ShouldStartDate(pawn, partner) && ShouldStartDate(partner, pawn))
                {
                    pawn.jobs.StopAll();
                    partner.jobs.StopAll();
                    if(pawn.GetLord() != null)
                    {
                        pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord);
                    }
                    if (partner.GetLord() != null)
                    {
                        partner.GetLord().Notify_PawnLost(partner, PawnLostCondition.ForcedToJoinOtherLord);
                    }
                    if (pawn.Awake() && partner.Awake())
                    {
                        LordMaker.MakeNewLord(this.pawn.Faction, new LordJob_Date(this.pawn, this.partner), this.pawn.Map, new Pawn[] { this.pawn, this.partner });
                    }
                    else if(pawn.Awake())
                    {
                        this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MissedDate, this.partner);
                    }
                    else if (partner.Awake())
                    {
                        this.partner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MissedDate, this.pawn);
                    }
                }
                else
                {
                    this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DateCancelled);
                    this.partner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DateCancelled);
                }
                this.pawn.health.RemoveHediff(this);
            }
        }

        private static bool ShouldStartDate(Pawn p, Pawn partner)
        {
            return !p.Downed && (p.needs == null || !p.needs.food.Starving)
                && p.health.hediffSet.BleedRateTotal <= 0f
                && p.needs.rest.CurCategory < RestCategory.Exhausted
                && !p.InAggroMentalState && !p.IsPrisoner
                && p.GetTimeAssignment() != TimeAssignmentDefOf.Work
                && !p.Drafted && p.Map == partner.Map;
        }

        public Pawn partner;
        public int hour;
        public int day;
    }
}
