using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _MentalBreaker
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this MentalBreaker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(MentalBreaker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect MentalBreaker.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        internal static bool CanDoRandomMentalBreaks(this MentalBreaker _this)
        {
            var _canDoRandomMentalBreaks = typeof(MentalBreaker).GetProperty("CanDoRandomMentalBreaks", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_canDoRandomMentalBreaks == null)
            {
                Log.ErrorOnce("Unable to reflect MentalBreaker.CanDoRandomMentalBreaks!", 0x12348765);
            }
            return (bool)_canDoRandomMentalBreaks.GetValue(_this, null);
        }

        internal static MentalBreakIntensity CurrentDesiredMoodBreakIntensity(this MentalBreaker _this)
        {
            var _currentDesiredMoodBreakIntensity = typeof(MentalBreaker).GetProperty("CurrentDesiredMoodBreakIntensity", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_currentDesiredMoodBreakIntensity == null)
            {
                Log.ErrorOnce("Unable to reflect MentalBreaker.CurrentDesiredMoodBreakIntensity!", 0x12348765);
            }
            return (MentalBreakIntensity)_currentDesiredMoodBreakIntensity.GetValue(_this, null);
        }

        internal static IEnumerable<MentalBreakDef> CurrentPossibleMoodBreaks(this MentalBreaker _this)
        {
            var _currentPossibleMoodBreaks = typeof(MentalBreaker).GetProperty("CurrentPossibleMoodBreaks", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_currentPossibleMoodBreaks == null)
            {
                Log.ErrorOnce("Unable to reflect MentalBreaker.CurrentPossibleMoodBreaks!", 0x12348765);
            }
            return (IEnumerable<MentalBreakDef>)_currentPossibleMoodBreaks.GetValue(_this, null);
        }

        // Token: 0x06002938 RID: 10552 RVA: 0x000E5844 File Offset: 0x000E3A44
        [DetourMethod(typeof(MentalBreaker), "TryDoRandomMoodCausedMentalBreak")]
        internal static bool _TryDoRandomMoodCausedMentalBreak(this MentalBreaker _this)
        {
            if (!_this.CanDoRandomMentalBreaks() || _this.GetPawn().Downed || !_this.GetPawn().Awake())
            {
                return false;
            }
            if (_this.GetPawn().Faction != Faction.OfPlayer && _this.CurrentDesiredMoodBreakIntensity() != MentalBreakIntensity.Extreme)
            {
                return false;
            }
            MentalBreakDef mentalBreakDef;
            if (!_this.CurrentPossibleMoodBreaks().TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(_this.GetPawn()), out mentalBreakDef))
            {
                return false;
            }

            int intensity = (int)_this.CurrentDesiredMoodBreakIntensity();
            Hediff hediff = _this.GetPawn().health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
            if (hediff != null)
            {
                hediff.Severity += 0.15f-(intensity*0.5f);
            }
            else if(Rand.Value <= (0.6f-(0.25f*intensity)))
            {
                hediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Anxiety, _this.GetPawn(), null);
                hediff.Severity = 0.75f-(intensity*0.25f);
                _this.GetPawn().health.AddHediff(hediff, null, null);
            }

            var RandomMentalBreakReason = typeof(MentalBreaker).GetMethod("RandomMentalBreakReason", BindingFlags.Instance | BindingFlags.NonPublic);
            Thought thought = (Thought)RandomMentalBreakReason.Invoke(_this, null);
            string reason = (thought == null) ? null : thought.LabelCap;
            _this.GetPawn().mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef.mentalState, reason, false, true, null);
            return true;
        }
    }
}
