using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Modding;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Special.PopUps;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace BuildingTools
{
    public class BuildingToolsPlugin : GamePlugin
    {
        public static AssetBundle bundle;
        public string assetBundlePath = "AssetBundles/buildingtools";

        private BlockSearchUI searchUI = new BlockSearchUI(new BlockSearch());
        private CalculatorUI calcUI = new CalculatorUI(new Calculator());

        public string name => "BuildingTools";

        public Version version => new Version("0.4.5");

        public void OnLoad()
        {
            SafeLogging.Log(Path.GetFullPath(assetBundlePath));
            bundle = AssetBundle.LoadFromMemory(Properties.Resources.buildingtools);
            SafeLogging.Log(string.Join(", ", bundle.GetAllAssetNames()));

            GameEvents.UpdateEvent += CreateKeyPressEvent(
                () => searchUI.ToggleGui(),
                () => Input.GetKeyDown(KeyCode.BackQuote) && cBuild.GetSingleton().buildMode != enumBuildMode.inactive).ToDRegularEvent();
            GameEvents.UpdateEvent += CreateKeyPressEvent(() => calcUI.ToggleGui(), false, KeyCode.Insert).ToDRegularEvent();
        }

        public void OnSave() { }

        public static void ShowError(Exception ex)
        {
            GuiPopUp.Instance.Add(new PopupInfo("BuildingTools Exception", ex.ToString()));
            SafeLogging.LogError(ex.ToString());
        }

        public static KeyPressEvent CreateKeyPressEvent(KeyPressEvent.DKeyPressEvent keyPressed, Func<bool> condition)
        {
            var ev = new KeyPressEvent(condition);
            ev.KeyPressed += keyPressed;
            return ev;
        }
        public static KeyPressEvent CreateKeyPressEvent(KeyPressEvent.DKeyPressEvent keyPressed, bool useEvent, params KeyCode[] keys)
        {
            KeyPressEvent ev = null;
            if (useEvent)
            {
                ev = new KeyPressEvent(() =>
                {
                    if (Event.current.type == EventType.KeyDown)
                    {
                        foreach (var key in keys)
                        {
                            if (Event.current.keyCode == key)
                                return true;
                        }
                    }
                    return false;
                });
            }
            else
            {
                ev = new KeyPressEvent(() =>
                {
                    foreach (var key in keys)
                    {
                        if (Input.GetKeyDown(key))
                            return true;
                    }
                    return false;
                });
            }
            ev.KeyPressed += keyPressed;
            return ev;
        }
    }

    public class VectorContractResolver : DefaultContractResolver
    {
        private static List<string> accepted = new List<string> { "x", "y", "z" };
        public static readonly VectorContractResolver Instance = new VectorContractResolver();

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

    public enum KeyInputs
    {
        BlockSearch = 12580,
        Calculator
    }
}
