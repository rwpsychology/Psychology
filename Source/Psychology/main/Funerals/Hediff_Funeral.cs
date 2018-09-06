using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;
using Verse;

namespace Psychology
{
    public class Hediff_Funeral : Hediff
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spot, "spot", default(IntVec3));
            Scribe_References.Look(ref grave, "grave");
            Scribe_Values.Look(ref date, "date", 0);
            Scribe_Values.Look(ref day, "day", 0);
            Scribe_Values.Look(ref hour, "hour", 0);
            Scribe_Values.Look(ref announced, "announced", true);
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        [LogPerformance]
        public override void Tick()
        {
            Pawn dead = null;
            base.Tick();
            if(dead == null)
            {
                Corpse corpse = (grave as Building_Grave).Corpse;
                if (corpse != null && corpse.InnerPawn != null)
                {
                    dead = corpse.InnerPawn;
                }
                else
                {
                    Log.Warning("[Psychology] A funeral was planned for someone, but they're gone now.");
                    pawn.health.RemoveHediff(this);
                }
            }
            if(!announced && ageTicks > GenDate.TicksPerHour)
            {
                Find.LetterStack.ReceiveLetter("LetterLabelFuneralPlanned".Translate(dead), "LetterFuneralPlanned".Translate(pawn, dead, GenDate.QuadrumDateStringAt(GenDate.TickGameToAbs(date), Find.WorldGrid.LongLatOf(pawn.Map.Tile).x), hour), LetterDefOf.PositiveEvent, pawn);
                announced = true;
            }
            if(GenLocalDate.DayOfYear(pawn.Map) >= day && GenLocalDate.HourOfDay(pawn.Map) == hour)
            {
                Find.LetterStack.ReceiveLetter("LetterLabelFuneralStarted".Translate(dead), "LetterFuneralStarted".Translate(dead), LetterDefOf.PositiveEvent, grave);
                LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_Funeral(spot, grave as Building_Grave), pawn.Map, null);
                pawn.health.RemoveHediff(this);
            }

        }

        public IntVec3 spot;
        public Thing grave;
        public int date;
        public int day;
        public int hour;
        public bool announced;
    }
}
