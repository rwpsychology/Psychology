using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _Building_Grave
    {
        [DetourMethod(typeof(Building_Grave),"Notify_CorpseBuried")]
        internal static void _Notify_CorpseBuried(this Building_Grave _this, Pawn worker)
        {
            CompArt comp = _this.GetComp<CompArt>();
            if (comp != null && !comp.Active)
            {
                comp.JustCreatedBy(worker);
                comp.InitializeArt(_this.Corpse.InnerPawn);
                worker.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.FilledGraveBleedingHeart);
            }
            _this.Map.mapDrawer.MapMeshDirty(_this.Position, MapMeshFlag.Buildings);
        }
    }
}
