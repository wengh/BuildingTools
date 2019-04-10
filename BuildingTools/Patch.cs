using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BrilliantSkies.Core.Control;
using BrilliantSkies.Core.FilesAndFolders;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Ftd.Avatar.Movement;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Examples.Pids;
using BrilliantSkies.Ui.Tips;
using BuildingTools.PIDTuner;
using Harmony;
using UnityEngine;

namespace BuildingTools
{
    public static class Patch
    {
        public static Dictionary<int, Tuner> Tuners { get; } = new Dictionary<int, Tuner>();

        private delegate bool PIDPrefixRef(float processVariable, float setPoint, float t, ref float ____lastt, ref float __result, PidStandardForm __instance);
        private delegate void GetFilesPostfixRef(ref IEnumerable<IFileSource> __result);

        public static void Apply()
        {
            var harmony = HarmonyInstance.Create("com.wengh.buildingtools");
            Debug.Log("BuildingTools Patching");

            harmony.Patch(
                typeof(PidGraphTab).GetMethod("Build", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
                prefix: new HarmonyMethod(((Action<PidGraphTab, VariableControllerMaster>)PIDTabPrefix).Method));
            Debug.Log("BuildingTools Patching Done 1/5");

            harmony.Patch(
                typeof(PidStandardForm).GetMethod("NewMeasurement", BindingFlags.Instance | BindingFlags.Public),
                prefix: new HarmonyMethod(((PIDPrefixRef)PIDPrefix).Method));
            Debug.Log("BuildingTools Patching Done 2/5");

            harmony.Patch(
                typeof(InventoryGUI).GetMethod("ChangeDimensions", BindingFlags.Instance | BindingFlags.NonPublic),
                transpiler: new HarmonyMethod(((Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>)PrefabTranspiler).Method));
            Debug.Log("BuildingTools Patching Done 3/5");

            harmony.Patch(
                typeof(OrbitingCamera).GetMethod("CheckScrollWheel", BindingFlags.Instance | BindingFlags.NonPublic),
                prefix: new HarmonyMethod(((Func<OrbitingCamera, bool>)OrbitCamPrefix).Method));
            Debug.Log("BuildingTools Patching Done 4/5");

            harmony.Patch(
                typeof(FilesystemFolderSource).GetMethod("GetFiles", BindingFlags.Instance | BindingFlags.Public),
                postfix: new HarmonyMethod(((GetFilesPostfixRef)GetFilesPostfix).Method));
            Debug.Log("BuildingTools Patching Done 5/5");
        }

        [HarmonyPatch(typeof(PidGraphTab))]
        [HarmonyPatch("Build")]
        public static void PIDTabPrefix(PidGraphTab __instance, VariableControllerMaster ____focus) // focus has 4 underscores
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
                    //Tuners[hash] = new TunerSGD(x, 0.05f, 0.005f);
                    Tuners[hash] = new TunerZieglerNichols(x);
                }
                else
                {
                    Tuners[hash].Interrupt();
                    Tuners.Remove(hash);
                }
            }));
        }

        [HarmonyPatch(typeof(PidStandardForm))]
        [HarmonyPatch("NewMeasurement")]
        public static bool PIDPrefix(float processVariable, float setPoint, float t,
            ref float ____lastt, ref float __result, PidStandardForm __instance)
        {
            var self = __instance;
            float input = processVariable;

            int hash = self.GetHashCode();

            if (!Tuners.TryGetValue(hash, out var tuner))
                return true;
            else if (!tuner.Initialized)
            {
                tuner.Initialize(input, __result);
            }

            if (tuner.Update(setPoint, input, t - ____lastt))
            {
                Traverse.Create(self).Property<float>("LastControlVariable").Value = tuner.Output;
                __result = tuner.Output;
                ____lastt = t;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(InventoryGUI))]
        [HarmonyPatch("ChangeDimensions")]
        public static IEnumerable<CodeInstruction> PrefabTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var i in instructions)
            {
                // Debug.Log(i.ToString() + i.operand?.GetType());
                if (i.opcode == OpCodes.Ldc_I4_S && i.operand as sbyte? == 64)
                {
                    i.opcode = OpCodes.Ldc_I4;
                    i.operand = int.MaxValue;
                }
            }
            return instructions;
        }

        [HarmonyPatch(typeof(OrbitingCamera))]
        [HarmonyPatch("CheckScrollWheel")]
        public static bool OrbitCamPrefix(OrbitingCamera __instance)
        {
            var self = __instance;
            float axis = Input.GetAxis("Mouse ScrollWheel");
            self.orbitDistance -= axis * 6f * Mathf.Max(1, self.orbitDistance / 30f);
            self.orbitDistance = Mathf.Clamp(self.orbitDistance, 1f, 300f);

            return false;
        }

        [HarmonyPatch(typeof(FilesystemFolderSource))]
        [HarmonyPatch("GetFiles")]
        public static void GetFilesPostfix(ref IEnumerable<IFileSource> __result)
        {
            __result = __result.AsParallel();
        }
    }
}
