using System;
using System.Collections.Generic;
using System.Reflection;
using BrilliantSkies.Core.Control;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Examples.Pids;
using BrilliantSkies.Ui.Tips;
using Harmony;
using UnityEngine;

namespace BuildingTools
{
    public static class Patch
    {
        public static Dictionary<int, PIDAutotune> Tuners { get; } = new Dictionary<int, PIDAutotune>();

        private delegate void ActionRef(float processVariable, ref float __result, PidStandardForm __instance);

        public static void Apply()
        {
            var harmony = HarmonyInstance.Create("com.wengh.buildingtools");
            Debug.Log("BuildingTools Patching");

            harmony.Patch(
                typeof(PidGraphTab).GetMethod("Build", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                prefix: new HarmonyMethod(((Action<PidGraphTab, VariableControllerMaster>)Prefix).Method));
            Debug.Log("BuildingTools Patching Done 1/2");

            harmony.Patch(
                typeof(PidStandardForm).GetMethod("NewMeasurement", BindingFlags.Instance | BindingFlags.Public),
                postfix: new HarmonyMethod(((ActionRef)Postfix).Method));
            Debug.Log("BuildingTools Patching Done 2/2");
        }

        [HarmonyPatch(typeof(PidGraphTab))]
        [HarmonyPatch("Build")]
        public static void Prefix(PidGraphTab __instance, VariableControllerMaster ____focus) // focus has 4 underscores
        {
            var self = __instance;
            var focus = ____focus;

            var seg1 = self.CreateStandardSegment();
            seg1.AddInterpretter(new SubjectiveButton<PidStandardForm>(focus.Pid,
                M.m<PidStandardForm>(x => Tuners.GetValueSafe(x.GetHashCode()) == null ? "Start Autotune" : "Stop Autotune"),
                M.m<PidStandardForm>(new ToolTip("Automatically adjust PID parameters (Kp, Ki, Kd)")), null, x =>
            {
                int hash = x.GetHashCode();
                if (!Tuners.ContainsKey(hash))
                {
                    Tuners[hash] = new PIDAutotune(0.05f, 0.005f);
                    Tuners[hash].SetTunings(x.kP, x.kI, x.kD);
                }
                else
                    Tuners.Remove(hash);
            }));
        }

        [HarmonyPatch(typeof(PidStandardForm))]
        [HarmonyPatch("NewMeasurement")]
        public static void Postfix(float processVariable, ref float __result, PidStandardForm __instance)
        {
            var self = __instance;
            float input = processVariable;

            int hash = self.GetHashCode();

            if (!Tuners.TryGetValue(hash, out var tuner))
                return;
            else if (!tuner.AutoMode)
            {
                tuner.EnableAuto(input, __result);
            }

            bool computed = tuner.Compute(self.LastSetPoint, input);

            self.kP = tuner.Kp;
            self.kI = tuner.Ki;
            self.kD = tuner.Kd;

            Traverse.Create(self).Property<float>("LastControlVariable").Value = tuner.Output;
            __result = tuner.Output;
        }
    }
}
