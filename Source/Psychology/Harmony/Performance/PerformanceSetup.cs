using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;
using Harmony;

namespace Psychology
{
    [StaticConstructorOnStartup]
    public static class PerformanceSetup
    {
        static PerformanceSetup()
        {
            if(PsychologyBase.EnablePerformanceTesting())
            {
                Log.Warning("Psychology :: Performance reporting is ON. Benchmarking itself has a performance impact. Unless you are opting into performance testing, disable it in the mod options.");
                HarmonyInstance harmony = HarmonyInstance.Create("rimworld.psychology.benchmarking");
                foreach (Type t in Assembly.GetAssembly(typeof(PsychologyBase)).GetTypes())
                {
                    foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (DoLogPerformance(m))
                        {
                            harmony.Patch(AccessTools.Method(t, m.Name),
                            new HarmonyMethod(AccessTools.Method(typeof(PerformancePatches), nameof(PerformancePatches.StopwatchStart))),
                            new HarmonyMethod(AccessTools.Method(typeof(PerformancePatches), nameof(PerformancePatches.StopwatchEnd))));
                        }
                    }
                }
            }
        }

        public static bool DoLogPerformance(MethodInfo m)
        {
            return m.GetCustomAttributes(true)
                    .Where(attr => attr is LogPerformanceAttribute)
                    .Count() > 0;
        }
        
        public static Dictionary<String, long> performanceTotals = new Dictionary<string, long>();
        public static Dictionary<String, int> performanceCalls = new Dictionary<string, int>();
    }
}
