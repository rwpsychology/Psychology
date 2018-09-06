using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class JobGiver_ArmSelf : ThinkNode_JobGiver
    {
        [LogPerformance]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.equipment == null)
            {
                return null;
            }
            IEnumerable<Thing> meleeWeapons = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon)
                                               where t.def.weaponTags != null && t.def.weaponTags.Contains("Melee") && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)
                                               orderby t.GetStatValue(StatDefOf.MeleeDPS) descending
                                               select t);
            if (meleeWeapons.Count() == 0)
            {
                return null;
            }
            Thing bestWeapon = meleeWeapons.First();
            if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.weaponTags.Contains("Melee") && pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeDPS) >= bestWeapon.GetStatValue(StatDefOf.MeleeDPS))
            {
                return null;
            }
            return new Job(JobDefOf.Equip, bestWeapon);
        }
    }
}
