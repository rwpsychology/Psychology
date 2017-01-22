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
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.equipment == null)
            {
                return null;
            }
            List<Thing> meleeWeapons = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon)
                                        where t.def.weaponTags != null && t.def.weaponTags.Contains("Melee") && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)
                                        select t).ToList();
            if (meleeWeapons.Count == 0)
            {
                return null;
            }
            meleeWeapons.SortByDescending((Thing x) => x.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount));
            Thing bestWeapon = meleeWeapons.First();
            float bestWeaponDPS = bestWeapon.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount) / bestWeapon.GetStatValue(StatDefOf.MeleeWeapon_Cooldown);
            if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.weaponTags.Contains("Melee") && pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount) / pawn.equipment.Primary.GetStatValue(StatDefOf.MeleeWeapon_Cooldown) >= bestWeaponDPS)
            {
                return null;
            }
            return new Job(JobDefOf.Equip, bestWeapon);
        }
    }
}
