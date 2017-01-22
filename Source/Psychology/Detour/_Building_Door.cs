using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Psychology.Detour
{
    internal static class _Building_Door
    {
        internal static FieldInfo _holdOpenInt;

        internal static void HoldOpenInt(this Building_Door _this, bool newValue)
        {
            if (_holdOpenInt == null)
            {
                _holdOpenInt = typeof(Building_Door).GetField("holdOpenInt", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_holdOpenInt == null)
                {
                    Log.ErrorOnce("Unable to reflect Building_Door.holdOpenInt!", 0x12348765);
                }
            }
            _holdOpenInt.SetValue(_this, newValue);
        }

        [DetourMethod(typeof(Building_Door),"PawnCanOpen")]
        internal static bool PawnCanOpen(this Building_Door d, Pawn p)
        {
            Lord lord = p.GetLord();
            return (lord != null && lord.LordJob != null && lord.LordJob.CanOpenAnyDoor(p)) || d.Faction == null || GenAI.MachinesLike(d.Faction, p) || p.health.hediffSet.HasHediff(HediffDefOfPsychology.Thief);
        }
    }
}
