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
    public class Recipe_TreatInsomniac : Recipe_Treatment
    {
        public Recipe_TreatInsomniac() : base(TraitDefOfPsychology.Insomniac, HediffDefOfPsychology.SleepingPills, "insomnia", 1f, TaleDefOfPsychology.TreatedInsomnia) { }
    }
}
