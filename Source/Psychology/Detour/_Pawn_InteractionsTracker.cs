using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _Pawn_InteractionsTracker
    {
        internal static FieldInfo _pawn;
        internal static FieldInfo _lastInteractionTime;

        internal static Pawn GetPawn(this Pawn_InteractionsTracker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Pawn_InteractionsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_InteractionsTracker.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        internal static int GetLastInteractionTime(this Pawn_InteractionsTracker _this)
        {
            if (_lastInteractionTime == null)
            {
                _lastInteractionTime = typeof(Pawn_InteractionsTracker).GetField("lastInteractionTime", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_lastInteractionTime == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_InteractionsTracker.lastInteractionTime!", 305432421);
                }
            }
            return (int)_lastInteractionTime.GetValue(_this);
        }

        [DetourMethod(typeof(Pawn_InteractionsTracker), "InteractedTooRecentlyToInteract")]
        internal static bool _InteractedTooRecentlyToInteract(this Pawn_InteractionsTracker _this)
        {
            return Find.TickManager.TicksGame < _this.GetLastInteractionTime() + 120 || _this.GetPawn().health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation);
        }
    }
}
