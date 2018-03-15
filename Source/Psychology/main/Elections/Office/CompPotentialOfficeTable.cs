using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology
{
    public class CompPotentialOfficeTable : ThingComp
    {
        //Working vars
        private bool active = false;

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                if (value == active)
                    return;

                active = value;

                if (parent.Spawned)
                {
                    if (active)
                    {
                        if (parent.Map.GetComponent<OfficeTableMapComponent>().officeTable != null && parent.Map.GetComponent<OfficeTableMapComponent>().officeTable != this)
                        {
                            parent.Map.GetComponent<OfficeTableMapComponent>().officeTable.Active = false;
                        }
                        parent.Map.GetComponent<OfficeTableMapComponent>().officeTable = this;
                    }
                    else
                    {
                        if (parent.Map.GetComponent<OfficeTableMapComponent>().officeTable == this)
                        {
                            parent.Map.GetComponent<OfficeTableMapComponent>().officeTable = null;
                        }
                    }
                }
            }
        }


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref active, "active", false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Active)
                parent.Map.GetComponent<OfficeTableMapComponent>().officeTable = this;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            if (Active)
            {
                map.GetComponent<OfficeTableMapComponent>().officeTable = null;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle com = new Command_Toggle();
            com.hotKey = KeyBindingDefOf.CommandTogglePower;
            com.defaultLabel = "CommandOfficeTableToggleLabel".Translate();
            com.icon = PsychologyTexCommand.OfficeTable;
            com.isActive = () => Active;
            com.toggleAction = () => Active = !Active;

            if (Active)
                com.defaultDesc = "CommandOfficeTableToggleDescActive".Translate();
            else
                com.defaultDesc = "CommandOfficeTableToggleDescInactive".Translate();

            yield return com;
        }
    }
}