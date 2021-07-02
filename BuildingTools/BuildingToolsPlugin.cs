using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Ftd.Avatar.Skills;
using BrilliantSkies.Modding;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles.Examples;
using BrilliantSkies.Ui.Special.PopUps;
using BuildingTools.Visualizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ui.Consoles.Examples;
using UnityEngine;

namespace BuildingTools
{
    public class BuildingToolsPlugin : GamePlugin
    {
        public static AssetBundle bundle;
        public string assetBundlePath = "AssetBundles/buildingtools";

        private MiscToolsUI toolUI = new();
        private CalculatorUI calcUI = new(new Calculator());

        public string name => "BuildingTools";

        public Version version => new("0.9.0");

        public void OnLoad()
        {
            AdvLogger.LogInfo(Path.GetFullPath(assetBundlePath));
            bundle = AssetBundle.LoadFromMemory(Properties.Resources.buildingtools);
            AdvLogger.LogInfo(string.Join(", ", bundle.GetAllAssetNames()));

            GameEvents.UpdateEvent.RegWithEvent(CreateKeyPressEvent(
                ts => toolUI.ToggleGui(),
                ts => BtKeyMap.Instance.IsKey(KeyInputsBt.BuildModeTools, KeyInputEventType.Down, ModifierAllows.CancelWhenUnnecessaryModifiers)
                   && cBuild.GetSingleton().buildMode != enumBuildMode.inactive));

            GameEvents.UpdateEvent.RegWithEvent(CreateKeyPressEvent(ts => calcUI.ToggleGui(), () => BtKeyMap.Instance.GetKeyDef(KeyInputsBt.Calculator)));

            GameEvents.UpdateEvent.RegWithEvent(CreateKeyPressEvent(ts =>
            {
                GuiPopUp.Instance.Add(new PopupConfirmation(
                    "Launch Armor Visualizer?",
                    "All <b>unsaved</b> constructs and level progress will be lost.",
                    x =>
                    {
                        if (x)
                            _ = new GameObject("ACVisualizer", typeof(ACVisualizer));
                    },
                    "<b>Continue</b>", "Cancel"));
            }, () => BtKeyMap.Instance.GetKeyDef(KeyInputsBt.ArmorVisualizer)));

            CoroutineLaunch.Invoke(() =>
            {
                if (BtSettings.Data.EnableNewFeaturesReport)
                {
                    ProfileManager.Instance.GetModule<ReceivedFeatures>().ShowPopup();
                    ProfileManager.Instance.Save(x => x is ReceivedFeatures);
                }
            }, 0.25f);

            FtdOptionsMenuUi.ExtraScreens.Add(window => new BtPanel(window));
        }

        public void OnSave() { }

        public static void ShowError(Exception ex)
        {
            AdvLogger.LogException("BuildingTools Exception", ex, LogOptions.Popup);
        }

        public static Action<ITimeStep> CreateKeyPressEvent(Action<ITimeStep> keyPressed, Func<ITimeStep, bool> condition)
        {
            return ts =>
            {
                if (condition(ts))
                    keyPressed(ts);
            };
        }
        public static Action<ITimeStep> CreateKeyPressEventUniversal(Action<ITimeStep> keyPressed, params KeyCode[] keys)
        {
            return ts =>
            {
                if (Event.current.type == EventType.KeyDown && keys.Contains(Event.current.keyCode))
                    keyPressed(ts);
            };
        }
        public static Action<ITimeStep> CreateKeyPressEvent(Action<ITimeStep> keyPressed, Func<KeyDef> key)
        {
            return ts =>
            {
                if (key().IsKey(KeyInputEventType.Down))
                    keyPressed(ts);
            };
        }
    }

    public class VectorContractResolver : DefaultContractResolver
    {
        private static List<string> accepted = new() { "x", "y", "z" };

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Vector3))
            {
                if (accepted.Contains(property.PropertyName))
                {
                    property.ShouldSerialize = x => true;
                }
                else
                {
                    property.ShouldSerialize = x => false;
                }
            }

            return property;
        }
    }
}
