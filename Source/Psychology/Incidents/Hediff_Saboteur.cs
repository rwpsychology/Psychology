using System;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using System.Reflection;
using Psychology.Detour;

namespace Psychology
{
    // Token: 0x02000188 RID: 392
    public class Hediff_Saboteur : HediffWithComps
    {

        public static Building_Turret FindTurretFor(Pawn p)
        {
            Predicate<Thing> turretValidator = delegate (Thing t)
            {
                Building_TurretGun building_Turret3 = (Building_TurretGun)t;
                if (!p.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some))
                {
                    return false;
                }
                return building_Turret3.GetComp<CompFlickable>().SwitchIsOn && !building_Turret3.IsBurning();
            };
            ThingDef thingDef = ThingDefOf.TurretGun;
            Predicate<Thing> validator = (Thing b) => turretValidator(b);
            Building_Turret building_Turret2 = (Building_Turret)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            if (building_Turret2 != null)
            {
                return building_Turret2;
            }
            return null;
        }

        public static Building FindBreakDownTargetFor(Pawn p)
        {
            Predicate<Thing> breakdownValidator = delegate (Thing t)
            {
                Building building3 = (Building)t;
                if (!p.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some))
                {
                    return false;
                }
                return !building3.GetComp<CompBreakdownable>().BrokenDown && !building3.IsBurning();
            };
            ThingDef thingDef = (from t in DefDatabase<ThingDef>.AllDefsListForReading
                                 where t.GetCompProperties<CompProperties_Breakdownable>() != null
                                 select t).ToList<ThingDef>().RandomElement();
            Predicate<Thing> validator = (Thing b) => breakdownValidator(b);
            Building building = (Building)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            if (building != null)
            {
                return building;
            }
            return null;
        }

        public override void Tick()
        {
            if (pawn.pather != null && pawn.pather.nextCell.GetDoor(pawn.Map) != null)
            {
                Building_Door d = pawn.pather.nextCell.GetDoor(pawn.Map);
                if (pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.Saboteur) & !d.HoldOpen && !pawn.Drafted && Rand.Value < 0.05f)
                {
                    d.HoldOpenInt(true);
                }
            }
            if (pawn.IsHashIntervalTick(10000) && Rand.Value < 0.25f)
            {
                switch(Rand.RangeInclusive(0, 10))
                {
                    case 0:
                        Building breakdownAble = FindBreakDownTargetFor(pawn);
                        if (breakdownAble != null)
                        {
                            pawn.jobs.StartJob(new Job(JobDefOfPsychology.Sabotage, breakdownAble), JobCondition.InterruptOptional, null, false, false, null);
                        }
                        break;
                    default:
                        Building_Turret turret = FindTurretFor(pawn);
                        if (turret != null)
                        {
                            turret.GetComp<CompFlickable>().WantSwitchOn(false);
                            turret.Map.designationManager.AddDesignation(new Designation(turret, DesignationDefOf.Flick));
                            pawn.jobs.StartJob(new Job(JobDefOf.Flick, turret), JobCondition.InterruptOptional, null, false, false, null);
                        }
                        break;
                }
            }
        }
    }
}
