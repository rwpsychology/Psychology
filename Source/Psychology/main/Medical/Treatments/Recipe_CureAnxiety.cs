using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Diagnostics;
using UnityEngine;

namespace Psychology
{
    public class Recipe_CureAnxiety : Recipe_Surgery
    {
        public Recipe_CureAnxiety() : base()
        {
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if(!CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                TaleRecorder.RecordTale(TaleDefOfPsychology.CuredAnxiety, new object[]
                {
                    billDoer,
                    pawn
                });
                Hediff anxiety = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
                pawn.health.RemoveHediff(anxiety);
                return;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if(pawn.RaceProps.Humanlike && pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.Anxiety))
            {
                List<BodyPartRecord> brain = new List<BodyPartRecord>();
                brain.Add(pawn.health.hediffSet.GetBrain());
                return brain;
            }
            return new List<BodyPartRecord>();
        }
    }
}
