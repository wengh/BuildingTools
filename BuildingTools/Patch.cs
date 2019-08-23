using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BrilliantSkies.Core.Control;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Ftd.Avatar.Movement;
using BrilliantSkies.Ftd.Avatar.Skills;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Examples;
using Harmony;
using UnityEngine;

namespace BuildingTools
{
    public static class Patch
    {
        private delegate bool PIDPrefixRef(float processVariable, float setPoint, float t, ref float ____lastt, ref float __result, PidStandardForm __instance);
        private delegate void KeyMapPostfixRef(KeyInputsFtd codedInput, KeyMap<KeyInputsFtd> __instance, ref bool __result);

        public static void Apply()
        {
            var harmony = HarmonyInstance.Create("com.wengh.buildingtools");
            Debug.Log("BuildingTools Patching");

            harmony.Patch(
                typeof(FtdOptionsMenuUi).GetMethod("BuildInterface", BindingFlags.Instance | BindingFlags.NonPublic),
                postfix: new HarmonyMethod(((Action<ConsoleWindow>)OptionsPostfix).Method));
            Debug.Log("BuildingTools Patched FtdOptionsMenuUi.BuildInterface");

            harmony.Patch(
                typeof(OrbitingCamera).GetMethod("CheckScrollWheel", BindingFlags.Instance | BindingFlags.NonPublic),
                prefix: new HarmonyMethod(((Func<OrbitingCamera, bool>)OrbitCamPrefix).Method));
            Debug.Log("BuildingTools Patched OrbitingCamera.CheckScrollWheel");

            harmony.Patch(
                typeof(SkillRollOff).GetMethod("CalcSkillAtLevel", BindingFlags.Instance | BindingFlags.Public),
                prefix: new HarmonyMethod(((Func<SkillRollOff, bool>)SkillPrefix).Method));
            Debug.Log("BuildingTools Patched SkillRollOff.CalcSkillAtLevel");

            if (Application.platform == RuntimePlatform.WindowsPlayer
             || Application.platform == RuntimePlatform.WindowsEditor)
            {
                harmony.Patch(
                    typeof(KeyMap<KeyInputsFtd>).GetMethod("IsKey", BindingFlags.Instance | BindingFlags.Public),
                    postfix: new HarmonyMethod(((KeyMapPostfixRef)CapsLock.KeyMapPostfix).Method));
                Debug.Log("BuildingTools Patched KeyMap<KeyInputsFtd>.IsKey");
            }
            else
            {
                Debug.Log("BuildingTools Skipped KeyMap<KeyInputsFtd>.IsKey");
            }
        }

        // FtdOptionsMenuUi.BuildInterface
        public static void OptionsPostfix(ConsoleWindow __result)
        {
            __result.AllScreens.Add(new BtPanel(__result));
        }

        // OrbitingCamera.CheckScrollWheel
        public static bool OrbitCamPrefix(OrbitingCamera __instance)
        {
            var self = __instance;
            float axis = Input.GetAxis("Mouse ScrollWheel");
            self.orbitDistance -= axis * 6f * Mathf.Max(1, self.orbitDistance / 30f);
            self.orbitDistance = Mathf.Clamp(self.orbitDistance, 1f, 300f);

            return false;
        }

        // SkillRollOff.CalcSkillAtLevel
        public static bool SkillPrefix(SkillRollOff __instance)
        {
            bool run = true;
            if (InstanceSpecification.i.Header.CommonSettings.DesignerOptions == DesignerOptions.FullDesigner && BtSettings.Data.DisableSkillsInDesigner)
            {
                __instance.CurrentValue = 1;
                run = false;
            }
            return run;
        }
    }
}
