using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;

namespace Psychology
{
    public static class _PsychologyBase
    {
        internal static FieldInfo _commonality;

        public static void SetCommonality(this TraitDef _this, float newValue)
        {
            if (_commonality == null)
            {
                _commonality = typeof(TraitDef).GetField("commonality", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_commonality == null)
                {
                    Log.ErrorOnce("Unable to reflect TraitDef.commonality!", 305432421);
                }
            }
            _commonality.SetValue(_this, newValue);
        }

        internal static FieldInfo _wantSwitchOn;

        internal static void WantSwitchOn(this CompFlickable _this, bool newValue)
        {
            if (_wantSwitchOn == null)
            {
                _wantSwitchOn = typeof(CompFlickable).GetField("wantSwitchOn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_wantSwitchOn == null)
                {
                    Log.ErrorOnce("Unable to reflect CompFlickable.wantSwitchOn!", 0x12348765);
                }
            }
            _wantSwitchOn.SetValue(_this, newValue);
        }
    }
}
