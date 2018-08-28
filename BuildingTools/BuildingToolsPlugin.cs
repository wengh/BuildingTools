using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Modding;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.Ftd.Avatar.Build;
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
        public static GlobalKeyListener Listener { get; }

        private BlockSearchUI searchUI = new BlockSearchUI(new BlockSearch());
        private CalculatorUI calcUI = new CalculatorUI(new Calculator());

        public string name => "BuildingTools";

        public Version version => new Version("0.4.1");

        public void OnLoad()
        {
            SafeLogging.Log(Path.GetFullPath(assetBundlePath));
            bundle = AssetBundle.LoadFromMemory(Properties.Resources.buildingtools);
            SafeLogging.Log(string.Join(", ", bundle.GetAllAssetNames()));

            GlobalKeyListener.Events.Add(CreateKeyPressEvent(
                () => Input.GetKeyDown(KeyCode.BackQuote) && cBuild.GetSingleton().buildMode != enumBuildMode.inactive,
                () => searchUI.ToggleGui()));

            GlobalKeyListener.Events.Add(CreateKeyPressEvent(KeyCode.Insert, false, () => calcUI.ToggleGui()));

            var keyMap = ProfileManager.Instance.GetModule<FtdKeyMap>();
            keyMap.TipDictionary[KeyInputs.BlockSearch.Ftd()] = "(BuildingTools) toggle Block Search UI in building mode";
            keyMap.TipDictionary[KeyInputs.Calculator.Ftd()] = "(BuildingTools) toggle Calculator UI";
            keyMap.SetIfNull(KeyInputs.BlockSearch.Ftd(), KeyCode.BackQuote, false);
            keyMap.SetIfNull(KeyInputs.Calculator.Ftd(), KeyCode.Insert, false);
        }

        public void OnSave() { }

        public static void ShowError(Exception ex)
        {
            GuiPopUp.Instance.Add(new PopupInfo("BuildingTools Exception", ex.ToString()));
            SafeLogging.LogError(ex.ToString());
        }

        public static KeyPressEvent CreateKeyPressEvent(Func<bool> condition, params KeyPressEvent.DKeyPressEvent[] keyPressed)
        {
            var ev = new KeyPressEvent(condition);
            foreach (var i in keyPressed)
                ev.KeyPressed += i;
            return ev;
        }
        public static KeyPressEvent CreateKeyPressEvent(KeyCode key, bool useEvent, params KeyPressEvent.DKeyPressEvent[] keyPressed) =>
            useEvent ?
                CreateKeyPressEvent(() => Event.current.type == EventType.KeyDown && Event.current.keyCode == key, keyPressed) :
                CreateKeyPressEvent(() => Input.GetKeyDown(key), keyPressed);
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
