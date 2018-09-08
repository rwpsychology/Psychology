using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Harmony;
using Verse;
using System.Reflection;

namespace Psychology
{
    class PerformancePatches
    {
        [HarmonyPriority(Priority.First)]
        public static bool StopwatchStart(ref Stopwatch __state)
        {
            __state = Stopwatch.StartNew();
            return true;
        }
        
        public static void StopwatchEnd(MethodBase __originalMethod, ref Stopwatch __state)
        {
            __state.Stop();
            long ticks = __state.ElapsedTicks;
            __state = null; //throw into garbage
            string fullName = __originalMethod.ReflectedType.Name + "." + __originalMethod.Name;
            //too lazy to DRY this
            if (!PerformanceSetup.performanceCalls.ContainsKey(fullName))
            {
                PerformanceSetup.performanceTotals[fullName] = ticks;
            }
            else
            {
                PerformanceSetup.performanceTotals[fullName] += ticks;
            }
            if (!PerformanceSetup.performanceCalls.ContainsKey(fullName))
            {
                PerformanceSetup.performanceCalls[fullName] = 1;
            }
            else
            {
                PerformanceSetup.performanceCalls[fullName] += 1;
            }
        }
    }
}
