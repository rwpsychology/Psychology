using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_GoHunting : ThinkNode_JobGiver
    {
        [LogPerformance]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null)
            {
                return null;
            }
            if (!WorkGiver_HunterHunt.HasHuntingWeapon(pawn))
            {
                IEnumerable<Thing> huntingWeapons = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon)
                                                   where t.def.IsRangedWeapon && t.TryGetComp<CompEquippable>().PrimaryVerb.HarmsHealth() && !t.TryGetComp<CompEquippable>().PrimaryVerb.UsesExplosiveProjectiles() && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)
                                                   orderby t.GetStatValue(StatDefOf.AccuracyLong) descending
                                                   select t);
                if (huntingWeapons.Count() > 0)
                {
                    Thing bestWeapon = huntingWeapons.First();
                    if (pawn.equipment.Primary == null || !pawn.equipment.Primary.def.IsRangedWeapon || !pawn.equipment.PrimaryEq.PrimaryVerb.HarmsHealth() || pawn.equipment.PrimaryEq.PrimaryVerb.UsesExplosiveProjectiles() || pawn.equipment.Primary.GetStatValue(StatDefOf.AccuracyLong) < bestWeapon.GetStatValue(StatDefOf.AccuracyLong))
                    {
                        return new Job(JobDefOf.Equip, bestWeapon);
                    }
                }
            }
            if (WorkGiver_HunterHunt.HasShieldAndRangedWeapon(pawn))
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    if (wornApparel[i] is ShieldBelt)
                    {
                        return new Job(JobDefOf.DropEquipment, wornApparel[i]);
                    }
                }
            }
            if(!(pawn.MentalState is MentalState_HuntingTrip))
            {
                return null;
            }
            Pawn assignedPrey = (pawn.MentalState as MentalState_HuntingTrip).prey;
            if(assignedPrey != null && !assignedPrey.Dead)
            {
                return new Job(JobDefOfPsychology.BreakHunt, assignedPrey);
            }
            IEnumerable<Pawn> wildlife = from p in Find.CurrentMap.mapPawns.AllPawns
                                         where p.Spawned && p.Faction == null && p.AnimalOrWildMan() && !p.Position.Fogged(p.Map) && pawn.CanReserve(p, 1, -1, null, true)
                                         select p;
            if (wildlife.Count() > 0)
            {
                Pawn prey = wildlife.RandomElement();
                (pawn.MentalState as MentalState_HuntingTrip).prey = prey;
                return new Job(JobDefOfPsychology.BreakHunt, prey);
            }
            return null;
        }
    }
}
