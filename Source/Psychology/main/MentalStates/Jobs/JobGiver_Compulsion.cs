using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology
{
    public class JobGiver_Compulsion : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null)
            {
                return null;
            }
            if (!pawn.story.WorkTagIsDisabled(WorkTags.Cleaning) && pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count > 0)
            {
                Thing closestFilth = pawn.Map.listerFilthInHomeArea.FilthInHomeArea.RandomElement();
                if (closestFilth != null && pawn.CanReserveAndReach(closestFilth, PathEndMode.Touch, Danger.Some))
                {

                    Job job = new Job(JobDefOf.Clean);
                    job.AddQueuedTarget(TargetIndex.A, closestFilth);
                    int num = 15;
                    Map map = closestFilth.Map;
                    Room room = closestFilth.GetRoom(RegionType.Set_Passable);
                    for (int i = 0; i < 100; i++)
                    {
                        IntVec3 intVec = closestFilth.Position + GenRadial.RadialPattern[i];
                        if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                        {
                            List<Thing> thingList = intVec.GetThingList(map);
                            for (int j = 0; j < thingList.Count; j++)
                            {
                                Thing thing = thingList[j];
                                if (thing != closestFilth && IsValidFilth(pawn, thing))
                                {
                                    job.AddQueuedTarget(TargetIndex.A, thing);
                                }
                            }
                            if (job.GetTargetQueue(TargetIndex.A).Count >= num)
                            {
                                break;
                            }
                        }
                    }
                    if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
                    {
                        job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
                    }
                    return job;
                }
            }
            if (!pawn.story.WorkTagIsDisabled(WorkTags.Hauling) && pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count > 0)
            {
                Thing thing = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().RandomElement();
                if (thing != null && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, thing, true) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
                {
                    return HaulAIUtility.HaulToStorageJob(pawn, thing);
                }
            }
            return null;
        }

        protected static bool IsValidFilth(Pawn pawn, Thing t)
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                return false;
            }
            Filth filth = t as Filth;
            if (filth == null)
            {
                return false;
            }
            if (!filth.Map.areaManager.Home[filth.Position])
            {
                return false;
            }
            LocalTargetInfo target = t;
            return pawn.CanReserve(target, 1, -1, null, true) && filth.TicksSinceThickened >= 600;
        }
    }
}
