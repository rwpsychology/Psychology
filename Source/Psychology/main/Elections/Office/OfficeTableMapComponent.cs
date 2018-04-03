using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Psychology
{
    class OfficeTableMapComponent : MapComponent
    {
        public CompPotentialOfficeTable officeTable;

        public OfficeTableMapComponent(Map map) : base(map)
        {
            officeTable = null;
        }
    }
}
