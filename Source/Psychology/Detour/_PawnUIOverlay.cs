using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using HugsLib.Source.Detour;
using UnityEngine;

namespace Psychology.Detour
{
    internal static class _PawnUIOverlay
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this PawnUIOverlay _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(PawnUIOverlay).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnUIOverlay.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        [DetourMethod(typeof(PawnUIOverlay),"DrawPawnGUIOverlay")]
        internal static void _DrawPawnGUIOverlay(this PawnUIOverlay _this)
        {
            if (!_this.GetPawn().Spawned || _this.GetPawn().Map.fogGrid.IsFogged(_this.GetPawn().Position) || _this.GetPawn().health.hediffSet.HasHediff(HediffDefOfPsychology.Thief))
            {
                return;
            }
            if (!_this.GetPawn().RaceProps.Humanlike)
            {
                switch (Prefs.AnimalNameMode)
                {
                    case AnimalNameDisplayMode.None:
                        return;
                    case AnimalNameDisplayMode.TameNamed:
                        if (_this.GetPawn().Name == null || _this.GetPawn().Name.Numerical)
                        {
                            return;
                        }
                        break;
                    case AnimalNameDisplayMode.TameAll:
                        if (_this.GetPawn().Name == null)
                        {
                            return;
                        }
                        break;
                }
            }
            Vector2 pos = GenMapUI.LabelDrawPosFor(_this.GetPawn(), -0.6f);
            GenMapUI.DrawPawnLabel(_this.GetPawn(), pos, 1f, 9999f, null, GameFont.Tiny, true, true);
            if (_this.GetPawn().CanTradeNow)
            {
                _this.GetPawn().Map.overlayDrawer.DrawOverlay(_this.GetPawn(), OverlayTypes.QuestionMark);
            }
        }
    }
}
