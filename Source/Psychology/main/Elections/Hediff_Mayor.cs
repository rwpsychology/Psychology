using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Psychology
{
    public class Hediff_Mayor : Hediff
    {
        public override void PostMake()
        {
            base.PostMake();
            this.yearElected = GenLocalDate.Year(this.pawn.Map.Tile);
            this.worldTileElectedOn = this.pawn.Map.Tile;
        }

        public override void Tick()
        {
            base.Tick();
            if (this.pawn.Dead || ((FactionBase)Find.WorldObjects.ObjectsAt(worldTileElectedOn).ToList().Find(o => o is FactionBase)).Map.mapConditionManager.ConditionIsActive(MapConditionDefOfPsychology.Election))
            {
                this.pawn.health.RemoveHediff(this);
            }

        }

        public int yearElected;
        public int worldTileElectedOn;
    }
}
