using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _Pawn_JobTracker
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this Pawn_JobTracker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Pawn_JobTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_JobTracker.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        [DetourMethod(typeof(Pawn_JobTracker),"CanTakeOrderedJob")]
        internal static bool _CanTakeOrderedJob(this Pawn_JobTracker _this)
        {
            Pawn pawn = _this.GetPawn();
            return (_this.curJob == null || _this.curJob.def.playerInterruptible) && !pawn.HasAttachment(ThingDefOf.Fire) && !(pawn.jobs.curDriver != null && pawn.jobs.curDriver.asleep && pawn.story.traits.HasTrait(TraitDefOfPsychology.HeavySleeper));
        }

        [DetourMethod(typeof(Pawn_JobTracker), "EndCurrentJob")]
        internal static void _EndCurrentJob(this Pawn_JobTracker _this, JobCondition condition, bool startNewJob = true)
        {
            if (_this.debugLog)
            {
                _this.DebugLogEvent(string.Concat(new object[]
                {
            "EndCurrentJob ",
            (_this.curJob == null) ? "null" : _this.curJob.ToString(),
            " condition=",
            condition,
            " curToil=",
            (_this.curDriver == null) ? "null_driver" : _this.curDriver.CurToilIndex.ToString()
                }));
            }
            var CleanupCurrentJob = typeof(Pawn_JobTracker).GetMethod("CleanupCurrentJob", BindingFlags.Instance | BindingFlags.NonPublic);
            var TryFindAndStartJob = typeof(Pawn_JobTracker).GetMethod("TryFindAndStartJob", BindingFlags.Instance | BindingFlags.NonPublic);
            if(_this.curDriver != null && _this.GetPawn().RaceProps.Humanlike && _this.curDriver.asleep && _this.GetPawn().needs.rest.GetLastRestTick() >= Find.TickManager.TicksGame - 200 && _this.GetPawn().story.traits.HasTrait(TraitDefOfPsychology.HeavySleeper))
            {
                return;
            }
            Job job = _this.curJob;
            CleanupCurrentJob.Invoke(_this, new object[] { condition, true, true });
            if (condition == JobCondition.ErroredPather || condition == JobCondition.Errored)
            {
                _this.StartJob(new Job(JobDefOf.Wait, 250, false), JobCondition.None, null, false, true, null);
                return;
            }
            if (startNewJob)
            {
                if (condition == JobCondition.Succeeded && job != null && job.def != JobDefOf.Wait && !_this.GetPawn().pather.Moving)
                {
                    _this.StartJob(new Job(JobDefOf.Wait, 1, false), JobCondition.None, null, false, false, null);
                }
                else
                {
                    TryFindAndStartJob.Invoke(_this, null);
                }
            }
        }
    }
}
