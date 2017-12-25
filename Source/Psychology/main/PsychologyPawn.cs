using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class PsychologyPawn : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if(this.RaceProps.Humanlike)
            {
                /* Fixes any improperly-configured psychologies. */
                if(this.psyche == null || this.psyche.PersonalityNodes == null)
                {
                    this.psyche = new Pawn_PsycheTracker(this);
                    this.psyche.Initialize();
                }
                foreach (PersonalityNode node in this.psyche.PersonalityNodes)
                {
                    if (node.rawRating < 0)
                    {
                        node.Initialize();
                    }
                }
                /* Same for sexuality. */
                if (this.sexuality == null && PsychologyBase.ActivateKinsey())
                {
                    this.sexuality = new Pawn_SexualityTracker(this);
                    this.sexuality.GenerateSexuality();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.sexuality, "sexuality", new object[]
            {
                this
            });
            Scribe_Deep.Look(ref this.psyche, "psyche", new object[]
            {
                this
            });
            Scribe_Values.Look(ref this.beenBuried, "beenBuried");
        }
        
        public override string LabelNoCount
        {
            get
            {
                if (this.Name == null)
                {
                    return this.KindLabel;
                }
                if (this.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor))
                {
                    return this.Name.ToStringShort + ", Mayor";
                }
                if (this.story == null || (this.story.adulthood == null && this.story.childhood == null))
                {
                    return this.Name.ToStringShort;
                }
                return this.Name.ToStringShort + ", " + this.story.TitleShort;
            }
        }

        public Pawn_SexualityTracker sexuality;
        public Pawn_PsycheTracker psyche;
        public bool beenBuried = false;
    }
}
