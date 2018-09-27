using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class CompPsychology : ThingComp
    {

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref this.sexuality, "sexuality", new object[]
            {
                this.parent as Pawn
            });
            Scribe_Deep.Look(ref this.psyche, "psyche", new object[]
            {
                this.parent as Pawn
            });
            /*Scribe_Deep.Look(ref this.recruiting, "recruiting", new object[]
            {
                this.parent as Pawn
            });*/
            Scribe_Values.Look(ref this.beenBuried, "beenBuried");
            Scribe_Values.Look(ref this.tickSinceLastSeenLover, "tickSinceLastSeenLover", Find.TickManager.TicksAbs);
        }

        public Pawn_SexualityTracker Sexuality
        {
            get
            {
                if(this.sexuality == null)
                {
                    Pawn p = this.parent as Pawn;
                    if (p != null)
                    {
                        this.sexuality = new Pawn_SexualityTracker(p);
                        this.sexuality.GenerateSexuality();
                    }
                    else
                    {
                        Log.Error("Psychology :: CompPsychology was added to " + this.parent.Label + " which cannot be cast to a Pawn.");
                    }
                }
                return this.sexuality;
            }
            set
            {
                this.sexuality = value;
            }
        }

        public Pawn_PsycheTracker Psyche
        {
            get
            {
                if (this.psyche == null)
                {
                    Pawn p = this.parent as Pawn;
                    if (p != null)
                    {
                        this.psyche = new Pawn_PsycheTracker(p);
                        this.psyche.Initialize();
                        foreach (PersonalityNode node in this.psyche.PersonalityNodes)
                        {
                            if (node.rawRating < 0)
                            {
                                node.Initialize();
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Psychology :: CompPsychology was added to " + this.parent.Label + " which cannot be cast to a Pawn.");
                    }
                }
                return this.psyche;
            }
            set
            {
                this.psyche = value;
            }
        }

        public bool AlreadyBuried
        {
            get
            {
                return this.beenBuried;
            }
            set
            {
                this.beenBuried = value;
            }
        }

        public int LDRTick
        {
            get
            {
                return tickSinceLastSeenLover;
            }
            set
            {
                tickSinceLastSeenLover = value;
            }
        }

        public bool isPsychologyPawn
        {
            get
            {
                return this.Psyche != null && this.Sexuality != null;
            }
        }
        
        private Pawn_SexualityTracker sexuality;
        private Pawn_PsycheTracker psyche;
        //public Pawn_TourMemories recruiting;
        private bool beenBuried = false;
        private int tickSinceLastSeenLover;
    }
}
