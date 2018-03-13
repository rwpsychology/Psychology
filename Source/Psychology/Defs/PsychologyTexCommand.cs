using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    [StaticConstructorOnStartup]
    class PsychologyTexCommand
    {
        public static readonly Texture2D OfficeTable = ContentFinder<Texture2D>.Get("UI/Commands/MayoralTable", true);
    }
}
