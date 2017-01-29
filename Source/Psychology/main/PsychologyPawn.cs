using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Psychology.Detour;

namespace Psychology
{
    public class PsychologyPawn : Pawn
    {
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            /* Adds psyche to PsychologyPawns on old saves
             * Also a stopgap fix for pawns spawning without psyches
             */
            if(this.psyche == null && this.RaceProps.Humanlike)
            {
                this.psyche = new Pawn_PsycheTracker(this);
                this.psyche.Initialize();
            }
            /* Same for sexuality. */
            if (this.sexuality == null && this.RaceProps.Humanlike && PsychologyBase.ActivateKinsey())
            {
                this.sexuality = new Pawn_SexualityTracker(this);
                _PawnGenerator.GenerateSexuality(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep(ref this.sexuality, "sexuality", new object[]
            {
                this
            });
            Scribe_Deep.LookDeep(ref this.psyche, "psyche", new object[]
            {
                this
            });
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
    }
}
