using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology
{
    internal static class _InteractionWorker_DeepTalk
    {
        [DetourMethod(typeof(InteractionWorker_DeepTalk),"RandomSelectionWeight")]
        internal static float _RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }
    }
}
