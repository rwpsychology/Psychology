using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Psychology
{
    // Token: 0x02000188 RID: 392
    public class Hediff_Anxiety : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            switch ((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % (50*(13-((this.CurStageIndex+1)*2))))
            {
                case 0:
                    panic = true;
                    this.Severity += 0.00000002f;
                    if (pawn.Spawned && pawn.RaceProps.Humanlike)
                    {
                        if (pawn.jobs.curJob.def != JobDefOf.FleeAndCower && !pawn.jobs.curDriver.asleep)
                        {
                            pawn.jobs.StartJob(new Job(JobDefOf.FleeAndCower, pawn.Position), JobCondition.InterruptForced, null, false, true, null);
                        }
                        else if (pawn.jobs.curDriver.asleep)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DreamNightmare);
                        }
                    }
                    break;
                default:
                    panic = false;
                    break;
            }
        }

        public bool panic = false;
    }
}
