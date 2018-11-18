using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class LordToil_HangOut : LordToil
    {
        public LordToil_HangOut(Pawn[] pawns)
        {
            this.friends = pawns;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfPsychology.HangOut, this.friends[0].Position, this.friends[1].Position, -1f);
            }
        }

        public Pawn[] friends;
        public Job hangOut;
        public int ticksToNextJoy = 0;
        public int tickSinceLastJobGiven = 0;
    }
}
