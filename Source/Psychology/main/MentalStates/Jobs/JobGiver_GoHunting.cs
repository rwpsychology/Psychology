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
                    if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.weaponTags.Contains("Melee") && pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeDPS) >= bestWeapon.GetStatValue(StatDefOf.MeleeDPS))
                    {
                        return null;
                    }
                    return new Job(JobDefOf.Equip, bestWeapon);
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
            IEnumerable<Pawn> wildlife = from p in Find.CurrentMap.mapPawns.AllPawns
                                         where p.Spawned && p.Faction == null && p.AnimalOrWildMan() && !p.Position.Fogged(p.Map)
                                         select p;
            return new Job(JobDefOf.Hunt, wildlife.RandomElement());
        }
    }
}
