using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    class RoomRoleWorker_Office : RoomRoleWorker
    {
        [LogPerformance]
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Thing thing = containedAndAdjacentThings[i];
                Building building_Table = thing as Building;
                if (building_Table != null && building_Table.def.HasComp(typeof(CompPotentialOfficeTable)) && building_Table.GetComp<CompPotentialOfficeTable>().Active)
                {
                    return 100005f;
                }
            }
            return 0f;
        }
    }
}
