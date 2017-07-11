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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.yearElected, "yearElected", 0);
            Scribe_Values.Look(ref this.worldTileElectedOn, "worldTileElectedOn", 0);
        }

        public override void PostMake()
        {
            base.PostMake();
            this.yearElected = GenLocalDate.Year(this.pawn.Map.Tile);
            this.worldTileElectedOn = this.pawn.Map.Tile;
        }

        public override void Tick()
        {
            base.Tick();
            FactionBase colony = Find.WorldObjects.ObjectsAt(worldTileElectedOn).OfType<FactionBase>().FirstOrDefault();
            if (this.pawn.Dead || !PsychologyBase.ActivateElections() || colony == null || colony.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
            {
                this.pawn.health.RemoveHediff(this);
            }

        }

        public int yearElected;
        public int worldTileElectedOn;
    }
}
